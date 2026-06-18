using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Imas
{
    static class CharacterManager
    {
        private static readonly Dictionary<string, List<Character>> _Characters = new();

        public static Character CreateCharacter(string characterId, string resourceId)
        {
            if (!_Characters.TryGetValue(characterId, out var list))
            {
                list = new List<Character>();
                _Characters[characterId] = list;
            }

            var go = new GameObject($"{characterId}_{list.Count}");
            var character = go.AddComponent<Character>();
            character.Initialize(characterId, resourceId);
            list.Add(character);
            return character;
        }

        public static List<Character> GetCharacters(string characterId) =>
            _Characters.TryGetValue(characterId, out var list) ? list : new List<Character>();

        public static List<Character> GetAllCharacters() =>
            _Characters.Values.SelectMany(list => list).ToList();

        public static void Clear()
        {
            foreach (var list in _Characters.Values)
            {
                foreach (var character in list)
                {
                    if (character != null)
                        Object.Destroy(character.gameObject);
                }
            }
            _Characters.Clear();
        }
    }
}
