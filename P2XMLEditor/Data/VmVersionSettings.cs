using System;

namespace P2XMLEditor.Data;

public record VmVersionSettings(
	string OutputPath,
	string GameName,
	Guid Scene,
	Guid WeatherSnapshot,
	DateTime SolarTime,
	int SkyRotation,
	int LoadingWindowGameDay,
	bool HideLoadingWindow,
	string LoadingScreenName
);
