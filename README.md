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
        - Children of a Childterm
          - Grandchildren of a Childterm
            - GrandChildren of a grandChild
              - GrandChild of Grandchildren

## when updating terms
- Updating deprecation status is supported
- Updating of labels is supported
- Changing of default label is supported

### Functional endpoints

`GET /api/termset` - return JSON array with all properties from default termstore  
`POST /api/termset` - takes JSON array with properties to be created as a new term or to update an existing term (created childterms on this route cannot be updated or added to the parentTerm after creation).
  - If the parentTerm does not exist, the request will complete by creating the parentTerm.
  -Afterwards grap the termId of the parentTerm, and set that in the payload you send, now you'll create your childterms. 

To create or update child terms use the below route :
`POST /api/termset/children` - takes JSON array with properties to be created as a new childterm or to update an existing childterm.

To create or update childs of a child term use the below route :
`POST /api/termset/child/children` - takes JSON array with properties to be created as a new child of a childterm or to update an existing child of a childterm.

To create or update childs of a child term use the below route :
`POST /api/termset/child/child/children` - takes JSON array with properties to be created as a new child of a childterm or to update an existing child of a childterm.

To create or update childs of a grandchild term use the below route :
`POST /api/termset/child/child/grandchild` - takes JSON array with properties to be created as a new grandchild of a childterm or to update an existing grandchild of a child term.

To create or update grandchildren of a grandchild term use the below route :
`POST /api/termset/child/grandchildren/grandchild` - takes JSON array with properties to be created as a new grandchild of a grandchild term or to update an existing grandchild of a grandchild term.

Entity shape to POST terms from `POST /api/termset`:

```json
[
{
      "termGroupName": "<Name of term group>",
      "termGroupId": "ef13220f-6950-46cb-8cec-0f3bb5be5a33",
      "termSetName": "<Name of term Set>",
      "termSetId": "e0f6f113-6857-4819-a383-c95cdb3c3a08",
      "termName": "<Name of term>",
      "termId": "56b95235-bfdb-40f2-9592-dc11a54754b5",
      "termLcid": 1033,
      "termChildTerms": [
          {}],
      "termLocalCustomProperties": {},
      "termCustomProperties": {},
      "termLabels": [
          {
              "isDefaultForLanguage": boolean [to add aliases here, set these to false],
              "language": int,
              "value": "<value>"
          },
          {
              "isDefaultForLanguage": false,
              "language": 1033,
              "value": "<value>"
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
  "_id": "termstore",
  "type": "system:microservice",
  "docker": {
    "environment": {
      "password": "<>",
      "url": "<>",
      "username": "<>"
    },
    "image": "<your docker name>/termstore:<a tag>",
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
    "system": "termstore",
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

