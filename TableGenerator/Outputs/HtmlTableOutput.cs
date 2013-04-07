﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AshMind.Extensions;
using DependencyInjection.TableGenerator.Data;

namespace DependencyInjection.TableGenerator.Outputs {
    public class HtmlTableOutput : IFeatureTableOutput {
        public void Write(DirectoryInfo directory, IEnumerable<FeatureTable> tables) {
            var builder = new StringBuilder();

            builder.AppendLine("<!DOCTYPE html>")
                   .AppendLine("<html>")
                   .AppendLine("  <head>")
                   .AppendLine("    <link rel='stylesheet' href='FeatureTests.css' />")
                   .AppendLine("  </head>")
                   .AppendLine("  <body>")
                   .AppendLine("    <div class='note'>Note: This file has been generated by TableGenerator and should not be edited manually.</div>");
            
            AppendTables(builder, tables);
            builder.AppendLine("  </body>")
                   .AppendLine("</html>");

            File.WriteAllText(Path.Combine(directory.FullName, "FeatureTests.html"), builder.ToString());

            // obviously the right way would be to embed it as resource, but IMHO it is good enough for
            // this type of project
            File.Copy(@"Outputs\FeatureTests.css", Path.Combine(directory.FullName, "FeatureTests.css"), true);
        }

        private void AppendTables(StringBuilder builder, IEnumerable<FeatureTable> tables) {
            foreach (var table in tables) {
                builder.AppendLine("    <table>")
                       .AppendLine("      <caption>")
                       .AppendFormat("        <div class='title'>{0}</div>", WebUtility.HtmlEncode(table.Name)).AppendLine()
                       .AppendFormat("        <div class='description'>{0}</div>", FormatTableDescription(table.Description)).AppendLine()
                       .AppendLine("      </caption>")
                       .AppendLine("      <tr>")
                       .AppendLine("        <th>Framework</th>");
                foreach (var feature in table.Features) {
                    builder.AppendFormat("        <th{0}>{1}</th>", CreateTitleAttribute(NormalizeUserProvidedDescription(feature.Description)), feature.Name).AppendLine();
                }
                builder.AppendLine("      </tr>");

                foreach (var row in table.GetRows()) {
                    builder.AppendLine("      <tr>")
                           .AppendFormat("        <td>{0}</td>", row.Item1.FrameworkName).AppendLine();
                    foreach (var cell in row.Item2) {
                        AppendCell(builder, cell);
                    }
                    builder.AppendLine("      </tr>");
                }
                builder.AppendLine("    </table>");
            }
        }

        private static void AppendCell(StringBuilder builder, FeatureCell cell) {
            var titleAttribute = CreateTitleAttribute(cell.Comment);
            var @class = cell.State.ToString().ToLowerInvariant();

            builder.AppendFormat("        <td{0} class='{1}'>{2}</td>", titleAttribute, @class, WebUtility.HtmlEncode(cell.Text))
                   .AppendLine();
        }

        private static string CreateTitleAttribute(string title) {
            if (title.IsNullOrEmpty())
                return "";

            return " title='" + WebUtility.HtmlEncode(title ?? "").Replace("\n", "&#10;").Replace("\r", "&#13;") + "'";
        }

        private string FormatTableDescription(string description) {
            var normalized = NormalizeUserProvidedDescription(description);
            return WebUtility.HtmlEncode(normalized);
        }

        private static string NormalizeUserProvidedDescription(string description) {
            var normalized = description ?? "";
            // replace all single new lines with spaces
            normalized = Regex.Replace(normalized, @"([^\r\n]|^)(?:\r\n|\r|\n)([^\r\n]|$)", "$1 $2");

            // collapse all spaces
            normalized = Regex.Replace(normalized, @" +", @" ");

            // remove all spaces at start/end of the line
            normalized = Regex.Replace(normalized, @"^ +| +$", "", RegexOptions.Multiline);
            return normalized;
        }
    }
}
