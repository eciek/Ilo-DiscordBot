using Newtonsoft.Json;
using DiscordBot.Modules.RaiderIO.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using Color = SixLabors.ImageSharp.Color;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;


namespace DiscordBot.Modules.RaiderIO
{
    public class RaiderIOService : InteractionModuleBase<SocketInteractionContext>
    {
        Font _baseFont;
        Font _nameFont;
        HttpClient _httpClient = new HttpClient();
        public RaiderIOService()
        {

            FontFamily fontFamily = new FontCollection().Add(Directory.GetCurrentDirectory() + "/Sheets/LifeCraft_Font.ttf");
            _nameFont = fontFamily.CreateFont(72, FontStyle.Regular);
            _baseFont = fontFamily.CreateFont(39, FontStyle.Regular);
        }
        public async Task<CharacterRIO> GetData(string charName, string realmName)
        {
            string apiLink = $"https://raider.io/api/v1/characters/profile?region=eu&realm={realmName}&name={charName}&fields=guild%2Cgear%2Craid_progression%2Cmythic_plus_scores_by_season%3Acurrent";
            HttpResponseMessage response = await _httpClient.GetAsync(apiLink);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                CharacterRIO character = JsonConvert.DeserializeObject<CharacterRIO>(responseBody);
                GenerateImage(character);
                return character;
            }
            else
            {
                return null;
            }
        }

        private void GenerateImage(CharacterRIO character)
        {
            using (Image<Rgba32> image = Image.Load<Rgba32>(Directory.GetCurrentDirectory() + "/Sheets/ScoreSheet.jpg"))
            {
                GenerateAOTC(image, character);
                GenerateATH(image, character);
                GenerateClass(image, character);
                GenerateFaction(image, character);
                GenerateGear(image, character);
                GenerateGender(image, character);
                GenerateGuild(image, character);
                GenerateName(image, character);
                GenerateRace(image, character);
                GenerateRealm(image, character);
                GenerateScore(image, character);
                GenerateScoreDPS(image, character);
                GenerateScoreHeal(image, character);
                GenerateScoreTank(image, character);
                GenerateSpec(image, character);
                GenerateVOTI(image, character);
                image.Save(Directory.GetCurrentDirectory() + $"/Sheets/{character.Name}.jpg");
            }
        }

        private void GenerateName(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_nameFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(621, 156),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.Name}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateClass(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(621, 295),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.Class}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateGuild(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(313, 295),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.Guild.Name}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateSpec(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(950, 295),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.ActiveSpecName}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateScore(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 366),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.MythicPlusScoresBySeason[0].Scores.All}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateRace(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 410),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.Race}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateGender(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 455),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.Gender}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateFaction(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 501),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.Faction}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateRealm(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 547),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.Realm}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateGear(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 592),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.Gear.ItemLevelEquipped}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateScoreDPS(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 638),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.MythicPlusScoresBySeason[0].Scores.Dps}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateScoreHeal(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 686),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.MythicPlusScoresBySeason[0].Scores.Healer}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateScoreTank(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 730),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.MythicPlusScoresBySeason[0].Scores.Tank}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateVOTI(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 796),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.RaidProgression.AwakenedVaultOfTheIncarnates.Summary}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateAOTC(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 882),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.RaidProgression.AwakenedAberrusTheShadowedCrucible.Summary}", Color.FromRgb(34, 32, 33)));
        }

        private void GenerateATH(Image image, CharacterRIO character)
        {
            RichTextOptions textOptions = new RichTextOptions(_baseFont)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Origin = new PointF(772, 967),
            };

            image.Mutate(ctx => ctx.DrawText(textOptions, $"{character.RaidProgression.AwakenedAmirdrassilTheDreamsHope.Summary}", Color.FromRgb(34, 32, 33)));
        }
    }
}
