using libMisterLauncher.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace libMisterLauncher.Entity
{
    internal static class GamelistXmlBuilder
    {
        public static string Build(string folderPath, List<(VideoGameDb game, RomDb rom)> entries, string publicBaseUrl)
        {
            var root = new XElement("gameList");

            // Un dossier virtuel par tag de playlist present parmi les jeux (roms primaires) de ce dossier.
            var tags = entries
                .Where(e => IsPrimaryRom(e.game, e.rom))
                .SelectMany(e => e.game.playlist ?? new List<string>())
                .Distinct()
                .ToList();

            foreach (var tag in tags)
            {
                root.Add(new XElement("folder",
                    new XElement("path", "./" + SanitizeTag(tag)),
                    new XElement("name", tag)));
            }

            foreach (var (game, rom) in entries)
            {
                if (string.IsNullOrEmpty(rom.fullpath))
                    continue;

                var relativePath = ToRelativePath(folderPath, rom.fullpath);

                if (IsPrimaryRom(game, rom))
                {
                    root.Add(BuildGameElement(relativePath, game, publicBaseUrl, parent: null));

                    // Un doublon minimal par tag de playlist, rattache au dossier virtuel via <parent>.
                    foreach (var tag in game.playlist ?? new List<string>())
                    {
                        root.Add(BuildGameElement(relativePath, game, publicBaseUrl, parent: "./" + SanitizeTag(tag)));
                    }
                }
                else
                {
                    // Clone (rom additionnelle du meme jeu) : entree minimale rattachee a la rom primaire,
                    // seulement si la rom primaire est aussi presente dans ce meme fichier.
                    var primaryRom = game.roms[0];
                    var primaryInThisFile = !string.IsNullOrEmpty(primaryRom.fullpath) && entries.Any(e => e.rom.romid == primaryRom.romid);
                    if (primaryInThisFile)
                    {
                        root.Add(new XElement("game",
                            new XElement("path", relativePath),
                            new XElement("name", game.name),
                            new XElement("parent", ToRelativePath(folderPath, primaryRom.fullpath))));
                    }
                    else
                    {
                        // La rom primaire est sur une autre racine physique (donc un autre gamelist.xml) :
                        // <parent> ne peut pas referencer un fichier externe, on duplique toutes les metadonnees.
                        root.Add(BuildGameElement(relativePath, game, publicBaseUrl, parent: null));
                    }
                }
            }

            var doc = new XDocument(new XDeclaration("1.0", null, null), root);
            return doc.ToString();
        }

        private static bool IsPrimaryRom(VideoGameDb game, RomDb rom)
        {
            return game.roms == null || game.roms.Count == 0 || game.roms[0].romid == rom.romid;
        }

        private static XElement BuildGameElement(string path, VideoGameDb game, string publicBaseUrl, string? parent)
        {
            var el = new XElement("game",
                new XElement("path", path),
                new XElement("name", game.name));

            if (!string.IsNullOrEmpty(game.desc))
                el.Add(new XElement("desc", game.desc));

            if (game.gamedate > DateTime.MinValue)
                el.Add(new XElement("releasedate", game.gamedate.ToString("yyyyMMdd'T'HHmmss")));

            if (!string.IsNullOrEmpty(game.developname))
                el.Add(new XElement("developer", game.developname));

            if (!string.IsNullOrEmpty(game.editorname))
                el.Add(new XElement("publisher", game.editorname));

            if (game.gametype != null && game.gametype.Count > 0)
                el.Add(new XElement("genre", string.Join("/", game.gametype)));

            if (!string.IsNullOrEmpty(game.nbplayers))
                el.Add(new XElement("players", game.nbplayers));

            if (game.rating.HasValue && game.rating.Value > 0)
                el.Add(new XElement("rating", (game.rating.Value / 20.0).ToString(CultureInfo.InvariantCulture)));

            var image = !string.IsNullOrEmpty(game.media_screenshot) ? game.media_screenshot : game.media_title;
            AddMediaElement(el, "image", image, publicBaseUrl);
            AddMediaElement(el, "video", game.media_video, publicBaseUrl);
            AddMediaElement(el, "manual", game.media_manuel, publicBaseUrl);

            if (!string.IsNullOrEmpty(parent))
                el.Add(new XElement("parent", parent));

            return el;
        }

        private static void AddMediaElement(XElement parent, string tagName, string? mediaId, string publicBaseUrl)
        {
            if (string.IsNullOrEmpty(mediaId))
                return;
            parent.Add(new XElement(tagName, $"{publicBaseUrl.TrimEnd('/')}/api/media/{mediaId}"));
        }

        private static string ToRelativePath(string folderPath, string fullpath)
        {
            if (fullpath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
                return "./" + fullpath.Substring(folderPath.Length);
            return "./" + fullpath;
        }

        private static string SanitizeTag(string tag)
        {
            return Tools.RemoveSpecialCaracteres(tag);
        }
    }
}
