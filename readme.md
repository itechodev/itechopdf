# wkhtmltoxcorelib 
wkhtmltoxcorelib  is a dotnet corelibrary around the wkhtmltopdf utility to produce PDF files from HTML. You can create multiple pages, cover pages and configurable header and footers.

## Installation
Package available on nuget.
```
dotnet add package wkhtmltoxcorelib 
```
```
PM> Install-Package wkhtmltoxcorelib 
```

## Example
```csharp
// First create a PDF renderer
var renderer = new PdfRenderer();
Console.WriteLine("WkHTML version:" + renderer.GetVersion());

// First add a cover page
var cover = new PdfDocument();
cover.Configure(print => {
    print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
    print.Orientation = Orientation.Landscape;
});
cover.FromFile("cover.html");

// then a content page with headers and footers of heigh 25mm and a spacing of 10mm
var content = new PdfDocument();
content.Configure(print => {
    print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
    print.Orientation = Orientation.Portrait;
});
content.AddFileHeader("res/header.html", 25, 10);
content.AddFileFooter("res/footer.html", 25, 10);
content.FromFile("res/content.html");

// Add these documents to the renderer and call renderToBytes
renderer.Add(cover);
renderer.Add(content);

var pdf = renderer.RenderToBytes();
File.WriteAllBytes("output.pdf", pdf);
```

## HTML Caveats and limitations
* Flex not supported
* SVG images should have explicit width and height
* Using a retina display on OSX will render very small.

## Why another library?
I needed a solution to produce rich PDF documents from HTML. By rich I mean: cover pages, multiple pages with potential different sizes and orientations, customizable headers and footers etc. None of the open source libraries could do that easily. There are commercial solutions but the pricetag scared me off, so I wrote my own.

## How does it work
Really simple actually.
1. HTML is inspected and changed to ensure correct paths, fix doctypes and add resource files including javascript and css. This is done using HtmlAgilityPack
2. Covert HTML to PDF using wkhtmltopdf
3. Merge all PDF's using PDFsharp

