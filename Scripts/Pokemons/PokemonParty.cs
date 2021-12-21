using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour {
    //Available pokemon in party
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons {
        get {
            return pokemons;
        }
    }
    //Add pokemon to party
    private void Start() {
        foreach (var pokemon in pokemons) {
            pokemon.Init();
        }
    }

    //Get first alive pokemon for battle
    //Where is a loop that will go through all the pokemons in party with HP greater than 0 -- Picks the first one
    public Pokemon GetHealthyPokemon() {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }
}
