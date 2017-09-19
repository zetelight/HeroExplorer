using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using HeroExplorer.Models;
using System.IO;
using System.Collections.ObjectModel;

namespace HeroExplorer
{
    // Facade is a speacial name in software development 
    // that is a design pattern for creating a layer of 
    // code over a dependency that you don't have control over.
    public class MarvelFacade
    {
        // Private and public key are deleted already
        // In order to get your own public/private key, please go to https://developer.marvel.com/ to find out details.
        private const string privateKey = "91d30e37180cbe37f159d67888c1018b6802ee64";
        private const string publicKey = "6bd8cd9eb013fbe28158b52cf566bc4d";
        private const int MAXHEROES = 500;
        private const string ImageNotAvailablePath = "http://i.annihil.us/u/prod/marvel/i/mg/b/40/image_not_available";

        public static async Task InitializeMarvelCharactersAsync(ObservableCollection<Character> marvelCharacters)
        {
            try
            {
                var characterDataWrapper = await GetCharacterDataWrapperAsync();

                // Get a list of characters
                var characters = characterDataWrapper.data.results;

                foreach (var character in characters)
                {
                    // Filter characters that miss thumbnail images
                    if (character.thumbnail != null
                        && character.thumbnail.path != ""
                        && character.thumbnail.path != ImageNotAvailablePath)
                    {

                        character.thumbnail.small = String.Format("{0}/standard_small.{1}",
                            character.thumbnail.path,
                            character.thumbnail.extension);

                        character.thumbnail.large = String.Format("{0}/portrait_xlarge.{1}",
                            character.thumbnail.path,
                            character.thumbnail.extension);

                        marvelCharacters.Add(character);
                    }
                }
            }
            catch(Exception)
            {
                return;
            }
 
        }

        public static async Task InitializeMarvelComicsAsync(int characterId, ObservableCollection<ComicBook> marvelComics)
        {
            try
            {
                var comicDataWrapper = await GetComicDataWrapperAsync(characterId);

                var comics = comicDataWrapper.data.results;

                foreach (var comic in comics)
                {
                    // Filter characters that are missing thumbnail images

                    if (comic.thumbnail != null
                        && comic.thumbnail.path != ""
                        && comic.thumbnail.path != ImageNotAvailablePath)
                    {

                        comic.thumbnail.small = String.Format("{0}/portrait_medium.{1}",
                            comic.thumbnail.path,
                            comic.thumbnail.extension);

                        comic.thumbnail.large = String.Format("{0}/portrait_xlarge.{1}",
                            comic.thumbnail.path,
                            comic.thumbnail.extension);

                        marvelComics.Add(comic);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private static async Task<CharacterDataWrapper> GetCharacterDataWrapperAsync()
        {
            // Assemble the URL
            Random random = new Random();
            var offset = random.Next(MAXHEROES);

            string url = String.Format("http://gateway.marvel.com:80/v1/public/characters?limit=10&offset={0}",
                offset);

            var jsonMessage = await CallMarvelAsync(url);

            // Response -> string / json -> deserialize
            var serializer = new DataContractJsonSerializer(typeof(CharacterDataWrapper));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonMessage));

            var result = (CharacterDataWrapper)serializer.ReadObject(ms);
            return result;
        }

        private static async Task<ComicDataWrapper> GetComicDataWrapperAsync(int characterId)
        {
            var url = String.Format("http://gateway.marvel.com:80/v1/public/comics?characters={0}&limit=10",
                characterId);

            var jsonMessage = await CallMarvelAsync(url);

            // Response -> string / json -> deserialize
            var serializer = new DataContractJsonSerializer(typeof(ComicDataWrapper));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonMessage));

            var result = (ComicDataWrapper)serializer.ReadObject(ms);
            return result;
        }

        private async static Task<string> CallMarvelAsync(string url)
        {
            // Get the MD5 Hash
            var timeStamp = DateTime.Now.Ticks.ToString();
            var hash = CreateHash(timeStamp);

            string completeUrl = String.Format("{0}&apikey={1}&ts={2}&hash={3}", url, publicKey, timeStamp, hash);

            // Call out to Marvel
            HttpClient http = new HttpClient();
            var response = await http.GetAsync(completeUrl);
            return await response.Content.ReadAsStringAsync();
        }

        private static string CreateHash(string timeStamp)
        {
            var toBeHashed = timeStamp + privateKey + publicKey;
            var hashedMessage = ComputeMD5(toBeHashed);
            return hashedMessage;
        }

        // From:
        // http://stackoverflow.com/questions/8299142/how-to-generate-md5-hash-code-for-my-winrt-app-using-c
        private static string ComputeMD5(string str)
        {
            // Get the algorithm to compute hash value
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

            // Encode string to binary buffer in UTF-8
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);

            // hash the binary buffer 
            var hashed = alg.HashData(buff);

            // Encode the hased buffer into hexadecimal codes, namely MD5 hash value.
            var res = CryptographicBuffer.EncodeToHexString(hashed);

            return res;
        }
    }
}
