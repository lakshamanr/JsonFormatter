using System.Collections.ObjectModel;
using JsonFormatterApp.Helpers;
using JsonFormatterApp.Services;

namespace JsonFormatterApp.ViewModels
{
    public class CompareViewModel : ViewModelBase
    {
        private readonly DiffService _diffService;

        public CompareViewModel(string leftTitle, string leftJson, string rightTitle, string rightJson)
        {
            _diffService = new DiffService();

            LeftTitle = leftTitle;
            LeftJson = leftJson;
            RightTitle = rightTitle;
            RightJson = rightJson;

            PerformComparison();
        }

        public string LeftTitle { get; }
        public string LeftJson { get; }
        public string RightTitle { get; }
        public string RightJson { get; }

        private ObservableCollection<string> _differences = new();
        public ObservableCollection<string> Differences
        {
            get => _differences;
            set => SetProperty(ref _differences, value);
        }

        private void PerformComparison()
        {
            try
            {
                var diffs = _diffService.GetSemanticDifferences(LeftJson, RightJson);

                if (diffs.Count == 0)
                {
                    Differences.Add("✓ No differences found - JSON files are identical!");
                }
                else
                {
                    Differences = new ObservableCollection<string>(diffs);
                }
            }
            catch (System.Exception ex)
            {
                Differences.Add($"Error comparing files: {ex.Message}");
            }
        }
    }
}
