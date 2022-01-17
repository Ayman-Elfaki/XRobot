
using namespace System



class ValidPortsGenerator : System.Management.Automation.IValidateSetValuesGenerator {
    [String[]] GetValidValues() {
        $Values = [System.IO.Ports.SerialPort]::getportnames()
        return $Values
    }
}

# I remembered that the code is written in assemply which makes it hardware dependant
# but my heart couldn't bear 😭😭 to delete the code responsible for generating board list after
# spending so much time on this bit 2>&1 

# class ValidBoardsGenerator : System.Management.Automation.IValidateSetValuesGenerator {
#     [String[]] GetValidValues() {
#         $arduinoConf = "${env:ProgramFiles(x86)}\Arduino\hardware\tools\avr\etc\avrdude.conf"
#         $parts = ((( avrdude.exe -C $arduinoConf -p ?) 2>&1) -split [System.Environment]::NewLine)
#         $Values = $parts[2..($parts.Length - 3)] | ForEach-Object -Process { $_.Substring($_.IndexOf("= ") + 1).Trim() } 
#         return $Values
#     }
# }

function Install-XRobot {
    
    [CmdletBinding()]
    param (
        [Parameter()]
        [ValidateSet([ValidPortsGenerator])]
        [string]
        $Port = "COM5",
        [Parameter()]
        [switch]
        $Upload
    )

    $Board = 'ATmega328P'
    $arduinoTooling = "${env:ProgramFiles(x86)}\Arduino\hardware\tools\avr\bin"
    $arduinoConf = "${env:ProgramFiles(x86)}\Arduino\hardware\tools\avr\etc\avrdude.conf"
    
    if (Test-Path $arduinoTooling) {
    
        if (-not ($env:Path -split ';' -contains $arduinoTooling)) {
            $env:Path += ";${arduinoTooling}"
        }
    
        .\tools\gavrasm.exe -lq .\src\Microcontroller\XRobot\Assembly\main.asm 
        Write-Host "Done Compiling To ${Board}." -ForegroundColor green

        if ($Upload) { 
            if (Test-Path -Path ".\src\main.hex" -PathType Leaf) {
                avrdude.exe -C $arduinoConf -v -p $Board.ToLowerInvariant()  -c arduino -P $Port -U flash:w:.\src\Microcontroller\XRobot\Assembly\main.hex:i
                Write-Host "Done Uploading To ${Board}." -ForegroundColor green
            }
            else {
                Write-Host "Errors Found." -ForegroundColor red
            }
        }
        
        if (Test-Path -Path ".\src\main.hex" -PathType Leaf) {
            Remove-Item .\src\Microcontroller\XRobot\Assembly\main.hex
        }
    
    }
    else {
        Write-Host "Arduino is not installed." -ForegroundColor red
    }
}