using System.Collections.Generic;

namespace DotNetCampus.Cli.Tests.Fakes
{
    public class OptionsParser : ICommandLineOptionParser<Options>
    {
        private bool _isFromCloud;
        private string? _filePath;
        private string? _startupMode;
        private bool _isSilence;
        private bool _isIwb;
        private string? _placement;
        private string? _startupSession;

        public string? Verb => null;

        public void SetValue(IReadOnlyList<string> values)
        {
            _filePath = values[0];
        }

        public void SetValue(char shortName, bool value)
        {
            switch (shortName)
            {
                case 's':
                    _isSilence = value;
                    break;
            }
        }

        public void SetValue(char shortName, string value)
        {
            switch (shortName)
            {
                case 'f':
                    _filePath = value;
                    break;
                case 'm':
                    _startupMode = value;
                    break;
                case 'p':
                    _placement = value;
                    break;
            }
        }

        public void SetValue(char shortName, IReadOnlyList<string> values)
        {
        }

        public void SetValue(string longName, bool value)
        {
            switch (longName)
            {
                case "Cloud":
                    _isFromCloud = value;
                    break;
                case "Silence":
                    _isSilence = value;
                    break;
                case "Iwb":
                    _isIwb = value;
                    break;
            }
        }

        public void SetValue(string longName, string value)
        {
            switch (longName)
            {
                case "File":
                    _filePath = value;
                    break;
                case "Mode":
                    _startupMode = value;
                    break;
                case "Placement":
                    _placement = value;
                    break;
                case "StartupSession":
                    _startupSession = value;
                    break;
            }
        }

        public void SetValue(string longName, IReadOnlyList<string> values)
        {
        }

        public Options Commit()
        {
            return new Options(_filePath, _isFromCloud, _startupMode, _isSilence, _isIwb, _placement, _startupSession);
        }
    }
}