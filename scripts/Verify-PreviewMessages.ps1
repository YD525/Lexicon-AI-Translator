[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectRoot
)

$resolvedRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$resourcePath = Join-Path $resolvedRoot 'Properties\Resources.resx'
$previewPath = Join-Path $resolvedRoot 'UIManagement\Preview'

[xml]$resources = Get-Content -LiteralPath $resourcePath -Raw
$catalogueIds = @{}
foreach ($entry in $resources.root.data) {
    $catalogueIds[$entry.name] = $true
}

if (-not (Test-Path -LiteralPath $previewPath)) {
    Write-Output 'Preview message verification passed: no preview views exist yet.'
    exit 0
}

$errors = New-Object System.Collections.Generic.List[string]
$visibleAttributePattern = '(?i)\b(?:Text|Content|Header|ToolTip|AutomationProperties\.Name)="(?!\{)([^"]*[A-Za-z][^"]*)"'
$messagePattern = '\{(?:[^{}]+:)?PreviewMessage\s+([A-Za-z][A-Za-z0-9_]*)\}'

foreach ($file in Get-ChildItem -LiteralPath $previewPath -Recurse -File -Filter '*.xaml') {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($match in [regex]::Matches($content, $visibleAttributePattern)) {
        $errors.Add("$($file.FullName): untracked visible XAML text '$($match.Groups[1].Value)'.")
    }

    foreach ($match in [regex]::Matches($content, $messagePattern)) {
        $id = $match.Groups[1].Value
        if (-not $catalogueIds.ContainsKey($id)) {
            $errors.Add("$($file.FullName): unknown preview message identifier '$id'.")
        }
    }
}

$codePattern = '(?i)(?:MessageBox\.Show|\.Content\s*=|\.Text\s*=|\.Header\s*=|\.ToolTip\s*=)\s*"[^"\r\n]*[A-Za-z][^"\r\n]*"'
foreach ($file in Get-ChildItem -LiteralPath $previewPath -Recurse -File -Filter '*.cs') {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($match in [regex]::Matches($content, $codePattern)) {
        $errors.Add("$($file.FullName): untracked visible code string '$($match.Value)'.")
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    throw "Preview message verification failed with $($errors.Count) error(s)."
}

Write-Output 'Preview message verification passed.'
