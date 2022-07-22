using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public class SinceLast : IComponent
    {
        protected InfoTextComponent InternalComponent { get; set; }
        public SinceLastSettings Settings { get; set; }
        public LiveSplitState State { get; set; }

        public float PaddingTop => InternalComponent.PaddingTop;
        public float PaddingLeft => InternalComponent.PaddingLeft;
        public float PaddingBottom => InternalComponent.PaddingBottom;
        public float PaddingRight => InternalComponent.PaddingRight;

        private string previousNameText { get; set; }

        public IDictionary<string, Action> ContextMenuControls => null;

        public SinceLast(LiveSplitState state)
        {
            Settings = new SinceLastSettings();
            State = state;
            InternalComponent = new InfoTextComponent("", "");
        }

        private void PrepareDraw(LiveSplitState state, LayoutMode mode)
        {
            InternalComponent.DisplayTwoRows = Settings.Display2Rows;

            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;

            InternalComponent.NameLabel.HorizontalAlignment = StringAlignment.Near;
            InternalComponent.ValueLabel.HorizontalAlignment = StringAlignment.Far;
            InternalComponent.NameLabel.VerticalAlignment =
                mode == LayoutMode.Horizontal || Settings.Display2Rows ? StringAlignment.Near : StringAlignment.Center;
            InternalComponent.ValueLabel.VerticalAlignment =
                mode == LayoutMode.Horizontal || Settings.Display2Rows ? StringAlignment.Far : StringAlignment.Center;

            InternalComponent.NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
            InternalComponent.ValueLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
        }

        private void DrawBackground(Graphics g, LiveSplitState state, float width, float height)
        {
            if (Settings.BackgroundColor.A > 0
                || Settings.BackgroundGradient != GradientType.Plain
                && Settings.BackgroundColor2.A > 0)
            {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == GradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == GradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            DrawBackground(g, state, width, VerticalHeight);
            PrepareDraw(state, LayoutMode.Vertical);
            InternalComponent.DrawVertical(g, state, width, clipRegion);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            DrawBackground(g, state, HorizontalWidth, height);
            PrepareDraw(state, LayoutMode.Horizontal);
            InternalComponent.DrawHorizontal(g, state, height, clipRegion);
        }

        public float VerticalHeight => InternalComponent.VerticalHeight;

        public float MinimumWidth => InternalComponent.MinimumWidth;

        public float HorizontalWidth => InternalComponent.HorizontalWidth;

        public float MinimumHeight => InternalComponent.MinimumHeight;

        public string ComponentName => "Since Last PB";

        public Control GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        private Attempt GetAttempt()
        {
            if (State.Run.AttemptHistory.Count <= 0)
            {
                Attempt TempAttempt = new Attempt();
                TempAttempt.Index = 0;
                return TempAttempt;
            }

            var bestTime = TimeSpan.MaxValue;
            int bestIndex = 0;
            foreach (Attempt attempt in State.Run.AttemptHistory)
            {
                if (attempt.Time[State.CurrentTimingMethod].HasValue)
                {
                    if (attempt.Time[State.CurrentTimingMethod].Value < bestTime)
                    {
                        bestTime = attempt.Time[State.CurrentTimingMethod].Value;
                        bestIndex = attempt.Index;
                    }
                }
            }
            if (bestIndex == 0)
                return State.Run.AttemptHistory[0];
            return State.Run.AttemptHistory[bestIndex - 1];
        }

        private int GetAttemptsSince()
        {
            if (State.CurrentPhase == TimerPhase.Ended)
            {
                if (State.Run.Last().Comparisons[Run.PersonalBestComparisonName][State.CurrentTimingMethod] == null || State.CurrentTime[State.CurrentTimingMethod] < State.Run.Last().Comparisons[Run.PersonalBestComparisonName][State.CurrentTimingMethod])
                    return 0;
            }
            Attempt pbattempt = GetAttempt();
            if (pbattempt.Index <= 0)
                return 0;
            int result = State.Run.AttemptHistory.Count - pbattempt.Index;
            if (State.CurrentPhase != TimerPhase.NotRunning)
                result += 1;
            return result;
        }

        private string GetDaysSince()
        {
            if (State.CurrentPhase == TimerPhase.Ended)
            {
                if (State.Run.Last().Comparisons[Run.PersonalBestComparisonName][State.CurrentTimingMethod] == null || State.CurrentTime[State.CurrentTimingMethod] < State.Run.Last().Comparisons[Run.PersonalBestComparisonName][State.CurrentTimingMethod])
                    return "0 days ago";
            }
            Attempt pbattempt = GetAttempt();
            if (pbattempt.Index <= 0)
                return "0 days ago";

            AtomicDateTime atomicnow = new AtomicDateTime(DateTime.Now, true);
            TimeSpan? since = atomicnow - pbattempt.Ended;
            if (since.HasValue)
            {
                int days = since.Value.Days;
                if (days == 1)
                    return "1 day ago";
                else
                    return days.ToString() + " days ago";
            }
            return "0 days ago";
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            string infoName;
            string infoValue;
            if (Settings.DisplayedInfo == DisplayedInfoType.DisplayDays)
            {
                infoName = "Days";
                infoValue = GetDaysSince();
            }
            else if (Settings.DisplayedInfo == DisplayedInfoType.DisplayAttempts)
            {
                infoName = "Attempts";
                infoValue = GetAttemptsSince().ToString();
            } else
            {
                infoName = "Days / Attempts";
                infoValue = GetDaysSince();
                infoValue += " / " + GetAttemptsSince().ToString();
            }
            infoName += " since last PB";
            InternalComponent.InformationName = infoName;
            InternalComponent.InformationValue = infoValue;
            InternalComponent.LongestString = infoName.Length > infoValue.Length
                ? infoName
                : infoValue;

            if (InternalComponent.InformationName != previousNameText)
            {
                InternalComponent.AlternateNameText.Clear();

                if (Settings.DisplayedInfo == DisplayedInfoType.DisplayAttempts)
                {
                    InternalComponent.AlternateNameText.Add("Attempts since last PB");
                    InternalComponent.AlternateNameText.Add("Attempts since PB");
                    InternalComponent.AlternateNameText.Add("A since PB");
                }
                else if (Settings.DisplayedInfo == DisplayedInfoType.DisplayDays)
                {
                    InternalComponent.AlternateNameText.Add("Days since last PB");
                    InternalComponent.AlternateNameText.Add("Days since PB");
                    InternalComponent.AlternateNameText.Add("D since PB");
                }
                else
                {
                    InternalComponent.AlternateNameText.Add("Days / Attempts since last PB");
                    InternalComponent.AlternateNameText.Add("Days / Attempts since PB");
                    InternalComponent.AlternateNameText.Add("D / A since last PB");
                    InternalComponent.AlternateNameText.Add("D / A since PB");
                }
                previousNameText = InternalComponent.InformationName;
            }

            InternalComponent.Update(invalidator, state, width, height, mode);
        }

        public void Dispose()
        {
        }

        public int GetSettingsHashCode() => Settings.GetSettingsHashCode();
    }
}
