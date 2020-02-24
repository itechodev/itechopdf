using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.files;
using org.pdfclown.documents.contents;
using actions = org.pdfclown.documents.interaction.actions;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.documents.interaction.navigation.document;
using files = org.pdfclown.files;
using org.pdfclown.objects;
using org.pdfclown.tools;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using org.pdfclown.documents.interaction.actions;

namespace ItechoPdf
{

    public class PdfEditor
    {

        public void Stuff(string filePath)
        {
            using (files::File file = new files::File(filePath))
            {

                Document document = file.Document;
                FindLinks(document);
            }
        }

        private void FindLinks(Document document)
        {
            // 2. Link extraction from the document pages.
            TextExtractor extractor = new TextExtractor();
            extractor.AreaTolerance = 2; // 2 pt tolerance on area boundary detection.
            foreach (Page page in document.Pages)
            {
                IDictionary<RectangleF?, IList<ITextString>> textStrings = null;

                // Get the page annotations!
                PageAnnotations annotations = page.Annotations;
                if (!annotations.Exists())
                {
                    Console.WriteLine("No annotations here.");
                    continue;
                }

                // Iterating through the page annotations looking for links...
                foreach (Annotation annotation in annotations)
                {
                    if (annotation is Link)
                    {
                        if (textStrings == null)
                        {
                            textStrings = extractor.Extract(page);
                        }

                        Link link = (Link)annotation;
                        RectangleF linkBox = link.Box;

                        // Text.
                        /*
                          Extracting text superimposed by the link...
                          NOTE: As links have no strong relation to page text but a weak location correspondence,
                          we have to filter extracted text by link area.
                        */
                        StringBuilder linkTextBuilder = new StringBuilder();
                        foreach (ITextString linkTextString in extractor.Filter(textStrings, linkBox))
                        {
                            linkTextBuilder.Append(linkTextString.Text);
                        }
                        Console.WriteLine("Link '" + linkTextBuilder + "' ");

                        // Position.
                        Console.WriteLine(
                          "    Position: "
                            + "x:" + Math.Round(linkBox.X) + ","
                            + "y:" + Math.Round(linkBox.Y) + ","
                            + "w:" + Math.Round(linkBox.Width) + ","
                            + "h:" + Math.Round(linkBox.Height)
                            );

                        Console.Write("    Target: ");
                        PdfObjectWrapper target = link.Target;
                        if (link.Target is actions::Action action)
                        {
                            if (action is GoToURI go)
                            {
                                Console.WriteLine($"URI is {go.URI.ToString()}");
                            }
                        }
                    }
                }
            }
        }


    }

}
