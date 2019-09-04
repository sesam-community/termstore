# sp-term-source

Simple service to fetch/create managed metadata (terms) on Sharepoint online

Works with Sharepoint Online - username/password authentication

### env vars
url - url for sharepoint tenant  
username - username to be used for authentication  
password - password to be used for authenticaiton 

### endpoints

`GET /api/termset` - return JSON array with all terms from default termstore  
`POST /api/termset` - takes JSON array with terms to be creates on input and creates trems on SharePoint 

Entity shape to POST new terms:
```json
[
    {
        "termSetId": "<term set id>",
        "termName": "<term set name>"
    },
    {
        "termSetId": "<term set id>",
        "termName": "<term set name>"
    }
]
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

