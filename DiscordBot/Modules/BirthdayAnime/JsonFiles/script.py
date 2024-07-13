import json
import re

def strip_character(characters):
    match = re.split("\),?",characters)
    if match:
        try:
            return match
        except:
            print (characters)
            return ''
    else:
        print (characters)
        return ''
    

def parse_character(character):
    match = re.match(r'^(.*?) \((.*?)\)?$', character.strip())
    if match:
        name_part = match.group(1)
        series = match.group(2)
        
        name_parts = name_part.split(' ')
        if len(name_parts) == 1:
            name = name_parts[0]
            surname = ''
        else:
            if len(name_parts) > 2:
                name = name_parts[0]
                surname = ' '.join(name_parts[1:])
            else:
                name, surname = name_parts

        return {
            'name': name,
            'surname': surname,
            'series': series
        }
        
    
    

with open('birthdayanime_old.json') as f:
    d = json.load(f)

with open('data.json', 'w', encoding='utf-8') as fs:


# Process the JSON data
    for entry in d:
        date = entry["date"]
        characters = entry["characters"]
        character_list = [char.strip() for char in strip_character(characters)]
        structured_characters = [parse_character(character) for character in character_list]
        structured_data =({"date": date, "characters": structured_characters})
        json.dump(structured_data, fs, ensure_ascii=False, indent=4)
# Convert characters field to list
        


# Print the new JSON structure

 
input("rxxxx")
f.close();