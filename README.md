# print-it

Windows service for printing files to a local or network printer in the background.

> Note: This repository differs from the upstream repo in a few key ways.
 * The route paths for the service now start with api/
 * The app can also be hosted as a normal web app instead of a windows service
 * Supports multiple file types (.pdf, .docx, images)

## Usage instructions
 ### Host as Service
1. Download the project, build it
2. Get the executable for your build from .\src\PrintIt.ServiceHost\bin place the package where you can find it. f.e. C:\print-it
3. Create print-it as a Windows service from an elevated command line: `sc create "PrintIt" binPath="C:\print-it\PrintIt.ServiceHost.exe" start=auto`
4. Start the service from the command line: `sc start PrintIt`
5. Check if the service is listening on port 7000 by running: `netstat -a | find ":7000"`

By default, _PrintIt.ServiceHost_ is listening on http://localhost:7000. The endpoint is configurable in _appsettings.json_.

 ### Host as Web App
1. Download the project, build it
2. Get the executable for your build from .\src\PrintIt.WebHost\bin 
3. Host as you'd like

> Note: A few things to beware of:
A user profile is needed to register printers. 
If you host this application in IIS, set the app pool to 'No Managed Code' and set Load User Profile = True
If you host the API on Server Core, don't forget to install printer support.
`Install-WindowsFeature Print-Services`

## Service Routes

#### [GET] api/printers/list

List all available printers on the system.

#### [POST] api/printers/install?printerPath=\\\\REMOTE_PC_NAME\\PRINTER-NAME

Install the network printer with the UNC-path `\\REMOTE_PC_NAME\PRINTER-NAME`. 

#### [POST] api/print

To print to a given printer, post a multipart form to this end-point with the following fields:

Field Name   | Required           | Content
------------ | ------------------ | ---------
PdfFile      | :heavy_check_mark: | The PDF file to print (Content-type: application/pdf)
PrinterPath  | :heavy_check_mark: | The UNC-path of the printer to send the PDF to
PageRange    |                    | An optional page range string (f.e. "1-5", "1, 3", "1, 4-8", "2-", "-5")

## PDFium

This project uses the [PDFium library](https://pdfium.googlesource.com/) for rendering the PDF file which is licensed under Apache 2.0, see [LICENSE](pdfium-binary/LICENSE).

The version included in this repository under the folder `pdfium-binary` was taken from https://github.com/bblanchon/pdfium-binaries/releases/tag/chromium/4194.
