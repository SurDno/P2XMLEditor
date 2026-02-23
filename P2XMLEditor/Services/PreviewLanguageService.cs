using System;
using System.Collections.Generic;
using System.Linq;

namespace P2XMLEditor.Services;

public static class PreviewLanguageService {
	private static string _currentLanguage = "english";

	public static string CurrentLanguage => _currentLanguage;

	public static event Action<string>? LanguageChanged;

	public static void SetLanguage(string language) {
		if (_currentLanguage == language) return;
		_currentLanguage = language;
		LanguageChanged?.Invoke(language);
	}


	public static void Initialise(HashSet<string>? languages) {
		if (languages == null || languages.Count == 0 || languages.Contains(_currentLanguage)) return;
		_currentLanguage = languages.First();
	}
}