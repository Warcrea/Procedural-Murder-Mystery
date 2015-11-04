﻿using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Family
{
    public string family_name;
    public List<Npc> family_members;
    public Npc husband;
    public Npc wife;
    public List<Npc> children;

    public Family(string name)
    {
        family_name = name;
        family_members = new List<Npc>();
        children = new List<Npc>();
    }
}

public class PlotGenerator : MonoBehaviour {

    public int number_of_characters = 8;
    public int max_families = 2;
    public int max_family_size = 3;

    public enum Motives {none, revenge, loverRevenge, jealousLove, inheritance}
    public Motives motive;
    public Npc victim, murderer;

    public List<Family> families;
    public List<Npc> npcs;

    public int[,] relationships;
    private readonly int nullRelationship = 100;

    //To be removed once a much better solution is found...
    //Change these to lists
    private List<string> firstnames_m;
    private List<string> firstnames_f;
    private List<string> surnames;
    
    // Use this for initialization
    [ContextMenu("Reset")]
    void Start () {
        families = new List<Family>();
        npcs = new List<Npc>();

        relationships = new int[number_of_characters, number_of_characters];
        for (int i = 0; i < number_of_characters; i++) {
            for (int x = 0; x < number_of_characters; x++) {
                relationships[i, x] = nullRelationship;
            }
        }

        victim = null; murderer = null;

        loadNames();
        generateCharacters();
        createFamilies();
        if (motive == Motives.none) selectMotive();
        prepareMotive();
        createRelationships();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void selectMotive()
    {
        int i;
        if (families.Count > 0) {
            i = Random.Range(0, 4);
        } else i = Random.Range(0, 3);

        switch (i) {
            case 0:
                motive = Motives.revenge;       //Killing over an event in the murderer's history
                break;
            case 1:
                motive = Motives.loverRevenge;  //Killing because the victim refused to love them back
                break;
            case 2:
                motive = Motives.jealousLove;   //Killing because the NPC they love was in love with the victim
                break;
            case 3:
                motive = Motives.inheritance;   //Killing a family member for the sake of gaining inheritance, either from the victim or from someone who would've otherwise given money to that victim
                break;
        }
    }

    void prepareMotive() {
        //revenge just requires one NPC to hate another
        if (motive == Motives.revenge){
            int m = Random.Range(0, number_of_characters);
            murderer = npcs[m];
            int v = 0;
            while (victim == null || victim == murderer) {
                v = Random.Range(0, number_of_characters);
                victim = npcs[v];
            }

            //Make murderer hate victim
            relationships[m,v] = -3;
        }
       
    }

    void generateCharacters(){
        for (int i = 0; i < number_of_characters; i++){
            npcs.Add(newCharacter());
        }
    }

    void createRelationships(){
        for(int i = 0; i < number_of_characters; i++) {
            for (int x = 0; x < number_of_characters; x++){
                if (i == x) {
                    relationships[i, i] = nullRelationship;
                }
                else if (relationships[i, x] == nullRelationship){
                    relationships[i, x] = Random.Range(-3, 4);
                    relationships[i, x] = Random.Range(-3, 4);
                }
                Debug.Log(npcs[i].firstname + " " + npcs[i].surname + " an attitude of " + relationships[i, x] + " towards " + npcs[x].firstname + npcs[x].surname);
            }
        }
    }

    Npc newCharacter() {
        Npc newNPC = new Npc();

        //Assign gender
        int r = Random.Range(0, 2);
        if (r == 0) { newNPC.gender = Npc.Gender.Male; } else { newNPC.gender = Npc.Gender.Female; }

        //Assign firstname
        if (newNPC.gender == Npc.Gender.Male) {
            r = Random.Range(0, firstnames_m.Count);
            newNPC.firstname = firstnames_m[r];
            firstnames_m.RemoveAt(r);
        }
        else {
            r = Random.Range(0, firstnames_f.Count);
            newNPC.firstname = firstnames_f[r];
            firstnames_f.RemoveAt(r);
        }

        return newNPC;
    }

    void createFamilies() {
        foreach (Npc npc in npcs) {
            /* Family logic
            If less families than the max, randomly attempt to create a family
            If random attempt failed or families at max, then randomly choose existing family and attempt to join
            If unsuccesful, just take a surname and remove it from the list
            If at the end of the NPC loop there are families with only one member, randomly choose a non family member to join. If none available, remove from that family and make the family name just a surname
            */

             //If there is room for a new family, attempt to make one
             if (families.Count < max_families)
             {
                //If attempt is succesful add the NPC to it and change their surname
                int r = Random.Range(0, 100);
                if (r > 60)
                {
                    r = Random.Range(0, surnames.Count);
                    string surname = surnames[r];
                    surnames.RemoveAt(r);

                    Family family = new Family(surname);

                    npc.family = family;
                    families.Add(family);
                    family.family_members.Add(npc);
                    npc.surname = surname;

                    if (npc.gender == Npc.Gender.Male) family.husband = npc;
                    else family.wife = npc;
                }
            }

            //If there is room in an existing family, try to join it
            if (npc.surname == null) {
                foreach (Family family in families) {
                    //If the family has room for children, or is missing a husband or wife and this NPC is the right gender, add to family
                    if (family.family_members.Count < max_family_size && ((family.husband != null && family.wife != null) || (npc.gender == Npc.Gender.Male && family.husband == null) || (npc.gender == Npc.Gender.Female && family.wife == null)) ) {
                        int r = Random.Range(0, 100);
                        if (r > 30) {
                            npc.family = family;
                            family.family_members.Add(npc);
                            npc.surname = family.family_name;

                            if (family.husband != null && family.wife != null) {
                                family.children.Add(npc);
                            }
                            else if (family.husband == null) {
                                family.husband = npc;
                            }
                            else if (family.wife == null) {
                                family.wife = npc;
                            }
                            break;
                        }
                    }
                }
            }

            //If not joined a family by this point, take a random surname
            if (npc.surname == null) {
                int i = Random.Range(0, surnames.Count);
                string randomSurname = surnames[i];
                surnames.RemoveAt(i);
                npc.surname = randomSurname;
            }
        }

        //IF there are any families with 1 member, add the first non-family npc to it. If none available, remove the family.

        for (int f = families.Count-1; f >= 0; f--) {
            if (families[f].family_members.Count == 1) {
                Debug.Log("found a family with one member");

                Npc.Gender searchingFor;
                if (families[f].wife == null) searchingFor = Npc.Gender.Female;
                else searchingFor = Npc.Gender.Male;

                bool stillSearching = true;
                int i = 0;

                while (stillSearching == true && i < npcs.Count-1){
                    if (npcs[i].family == null && npcs[i].gender == searchingFor)
                    {
                        npcs[i].family = families[f];
                        families[f].family_members.Add(npcs[i]);
                        npcs[i].surname = families[f].family_name;

                        if (searchingFor == Npc.Gender.Male) families[f].husband = npcs[i];
                        else families[f].wife = npcs[i];

                        Debug.Log("added " + npcs[i].firstname + "to the " + families[f].family_name +"family");

                        stillSearching = false;
                    }
                    i++;
                }

                if (stillSearching)
                {
                    Debug.Log("No suitable npcs found, removing " + families[f].family_name +" family");
                    families[f].family_members[0].family = null;
                    families.Remove(families[f]);
                }
            }
        }
    }

    void loadNames()
    {
        firstnames_m = new List<string> {
            "Miguel",
            "Osvaldo",
            "Reinaldo",
            "Roy",
            "Moses",
            "Hugh",
            "Rocky",
            "Austin",
            "Walter",
            "Gustavo",
            "Columbus",
            "Antony",
            "Angel",
            "Sterling",
            "Carlton",
            "Andrew",
            "Jack",
            "Franklin",
            "George",
            "David"
        };

        firstnames_f = new List<string> {
            "Sally",
            "Lucille",
            "Betsey",
            "Hattie",
            "Loni",
            "Dorathy",
            "Krystle",
            "Iona",
            "Tyra",
            "Jeanna",
            "Lucy",
            "Sarah",
            "Julie",
            "Erica",
            "Chrissie",
            "Laura",
            "Samantha",
            "Beatrice",
            "Edith",
            "Gloria"
        };

        surnames = new List<string>
        {
            "Smith",
            "Jones",
            "Williams",
            "Lawrence",
            "Morse",
            "Lewis",
            "Davis",
            "Murphy",
            "Price",
            "Cole"
        };
    }
}
