$versionInfoTemplatePath = ".\VersionInfoTemplate.cs"
$versionInfoPath = ".\VersionInfo.cs"

$currentDate = Get-Date -Format "yyyy-MM-dd"
$currentTime = Get-Date -Format "HH:mm"
$timeZone = Get-TimeZone
$timeId = $timeZone.Id
$content = Get-Content $versionInfoTemplatePath
$content = $content -replace 'BuiltOnDate = "";', "BuiltOnDate = `"$currentDate`";"
$content = $content -replace 'BuiltOnTime = "";', "BuiltOnTime = `"$currentTime`";"
$content = $content -replace 'BuiltTimeZone = "";', "BuiltTimeZone = `"$timeId`";"
$content = $content -replace 'class VersionInfoTemplate', 'class VersionInfo'
$content = $content -replace '// Do not change consts!', '// Use VersionInfoTemplate.cs!'
Set-Content $versionInfoPath $content
