# Termstore

Simple service to fetch/create/update managed metadata (terms) on Sharepoint online

Works with Sharepoint Online - username/password authentication

### env vars
url - url for sharepoint tenant

username - username to be used for authentication

password - password to be used for authenticaiton 

### Functional endpoints

`GET /api/termset` - return JSON array with all terms from default termstore  
`POST /api/termset` - takes JSON array with terms to be created as a new term or to update an existing term.
- **When updating an existing term, you must set one of the defined aliases to true, as this will take the new name for that term...**

Entity shape to POST terms:

```json
[
	{
		"termGroupName": "<term-group>",
	    "termGroupId": "<id-int-string-sequence>",
	    "termSetName": "<term-group>",
	    "termSetId": "id-int-string-sequence>",
	    "termName": "string",
	    "termId": "id-int-string-sequence>",
	    "termLcid": int,
	    "termDescription": "string of length [0-100]",
	    "termIsAvailableForTagging": boolean,
	    "termLocalCustomProperties": {"key":"value"},
	    "termCustomProperties": {"Key": "value"},
	    "termIsDeprecated": boolean,
	    "termLabels": [
	        {
	            "isDefaultForLanguage": boolean [to add aliases here, set these to false, but one to true [!!Really important!!]],
	            "language": int,
	            "value": "string"
	        },
	        {
	        	"isDefaultForLanguage": boolean,
	            "language": int,
	            "value": "string"	
	        }
	    ]
}]
```

### Build  
To build docker image run from solution directory `docker build  -f ./SP_Taxonomy_client_test/Dockerfile .`

To build locally - use Visual Studio >=2019 Preview and .NET Core >= 3.0 Preview 

### Sesam system setup 

```json
{
  "_id": "net-core-test",
  "type": "system:microservice",
  "docker": {
    "environment": {
      "password": "<>",
      "url": "<>",
      "username": "<>"
    },
    "image": "ohuenno/sp-online-termstore",
    "port": 80
  },
  "verify_ssl": true
}
```

### Sesam pipe setup

```json
{
  "_id": "test-termstore",
  "type": "pipe",
  "source": {
    "type": "json",
    "system": "net-core-test",
    "url": "/api/termset"
  },
  "transform": {
    "type": "dtl",
    "rules": {
      "default": [
        ["copy", "*"],
        ["add", "_id", "_S.termId"]
      ]
    }
  }
}
```

