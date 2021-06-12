using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace HusicBasic.Models.Tasks
{
    public class TestTask : TaskBase, ITaskMinMaxAware<int>, ITaskEtaAware
    {
        #region Properties
        private int _Min = 0;
        private int _Max = 100;
        private bool _KnowsProgress = true;
        private bool _KnowsEta = true;
        private TimeSpan _Eta = TimeSpan.FromSeconds(10);
        #endregion

        #region Properties
        public int Min { get => _Min; private set => SetProperty(ref _Min, value, RaiseProgressChanged); }
        public int Max { get => _Max; private set => SetProperty(ref _Max, value, RaiseProgressChanged); }
        public double Progress => _Max == 0 ? 0 : _Min / (double)_Max * 100d;
        public TimeSpan ETA { get => _Eta; private set => SetProperty(ref _Eta, value); }
        public bool KnowsProgress { get => _KnowsProgress; private set => SetProperty(ref _KnowsProgress, value); }
        public bool KnowsETA { get => _KnowsEta; private set => SetProperty(ref _KnowsEta, value); }
        private DispatcherTimer Timer;
        private Dispatcher Disp;
        #endregion
        public TestTask() : base("Test", App.Current.FindResource("Text.Web") as string, "Test")
        {
            Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.1) };
            Timer.Tick += Timer_Tick;
        }

        #region Events
        private void Timer_Tick(object sender, EventArgs e)
        {
            Disp.Invoke(() => {
                if (Min == Max)
                {
                    Min = 0;
                    ETA = TimeSpan.FromMinutes(Max).Add(TimeSpan.FromSeconds(Max));
                    KnowsETA = false;
                    KnowsProgress = false;
                    Timer.Interval = TimeSpan.FromSeconds(5);
                }
                else
                {
                    Min++;
                    ETA = TimeSpan.FromMinutes(Max - Min).Add(TimeSpan.FromSeconds(Max - Min));
                    KnowsETA = true;
                    KnowsProgress = true;
                    Timer.Interval = TimeSpan.FromSeconds(0.1);
                }
            });
        }
        #endregion

        #region Methods
        private void RaiseProgressChanged() => RaisePropertyChanged(nameof(Progress));
        protected override void ParametersChanged() { }
        protected override void StartMain(Dispatcher disp)
        {
            Disp = disp;
            Timer.Start();
        }
        #endregion
    }
}
