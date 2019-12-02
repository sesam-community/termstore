# Termstore

Simple service to fetch/create/update managed metadata (terms) on Sharepoint online

Works with Sharepoint Online - username/password authentication

### env vars
url - url for sharepoint tenant

username - username to be used for authentication

password - password to be used for authenticaiton

## pitfalls

- It may seem you support other languages than 1033 (English), but when creating aliases, Termstore seems somewhat sensitive with regards to languages. Therefore you should stay with 1033 for now.

## supported term dimensions on total # of routes
- Termgroup
  - TermSet
    - Term
      - Childterm

## when updating terms
- Only "Term (aka ParentTerm)" can be updated, when using the `POST /api/termset` endpoint. Remember to provide the termId when updating a Term. In essense, this means that the endpoint does not currently support updating of "Childterms" after creation.
- You have to provide termCustomProperties and termLocalCustomProperties in the payload when updating a Term, otherwise the Term will not be updated.
    - an empty dict works fine.

## when updating child terms
- Childterms also need the cpChildLocalCustomProperties and cpChildCustomProperties to be updated.
- Remember to also provide the cpChildId when updating a childTerm.

### Functional endpoints

`GET /api/termset` - return JSON array with all properties from default termstore  
`POST /api/termset` - takes JSON array with properties to be created as a new term or to update an existing term (created childterms on this route cannot be updated or added to the parentTerm after creation).

To exclusively create or update child terms use the below route :
`POST /api/termset/children` - takes JSON array with properties to be created as a new childterm or to update an existing childterm.

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
	            "isDefaultForLanguage": boolean [to add aliases here, set these to false],
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

