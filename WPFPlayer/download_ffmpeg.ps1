param(
    [Parameter()]
    [string]$ProjectDir,

    [Parameter()]
    [string]$OutputDir
)

if(Test-Path -Path "$($OutputDir)ffmpeg/ffmpeg.exe") {
    Write-Host "ffmpeg binary exists! skip!"
    return
}

$ffmpeg_dir = "$($ProjectDir)ffmpeg\"
if( -not (Test-Path -Path "$($ffmpeg_dir)ffmpeg.exe") )
{
    $ffmpeg_url = "https://github.com/BtbN/FFmpeg-Builds/releases/download/autobuild-2022-09-30-12-41/ffmpeg-n4.4.2-95-ga8f16d4eb4-win64-gpl-shared-4.4.zip"
    $ffmpeg_zip_file = "$($OutputDir)ffmpeg.zip"
    $ffmpeg_temp_dir = "$($OutputDir)ffmpeg_temp\"

    Write-Host "Download ffmpeg binary to $($ffmpeg_zip_file)"
    Invoke-WebRequest -Uri $ffmpeg_url -OutFile $ffmpeg_zip_file

    Write-Host "Unzipping ffmpeg binary to $($ffmpeg_temp_dir)"
    Expand-Archive -Path $ffmpeg_zip_file -DestinationPath $ffmpeg_temp_dir -Force
    $subdir = (Get-ChildItem -Path $ffmpeg_temp_dir -Directory)[0]

    Write-Host "Copy ffmpeg binary to $($ffmpeg_dir)"
    Copy-Item -Path "$($ffmpeg_temp_dir)$($subdir)\bin\" -Destination "$($ffmpeg_dir)" -Recurse -Force

    Remove-Item -Path $ffmpeg_zip_file -Force
    Remove-Item -Path $ffmpeg_temp_dir -Force -Recurse
}

Write-Host "Copy ffmpeg binary to $($OutputDir)ffmpeg\"
Copy-Item -Path "$($ffmpeg_dir)" -Destination "$($OutputDir)ffmpeg\" -Recurse -Force
