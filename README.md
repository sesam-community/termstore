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

### Updating is currently deprecated (16.01.2020 - commit "minimizing memory usage")
## when updating terms
- The "Term (aka ParentTerm)" can only be updated, when using the `POST /api/termset` endpoint. Remember to provide the termId when updating a Term. In essense, this means that the endpoint does not currently support updating of "Childterms" after creation.
- You have to provide termCustomProperties and termLocalCustomProperties in the payload when updating a Term, otherwise the Term will not be updated.
    - an empty dict works fine.

## when updating child terms
- Childterms also need the cpChildLocalCustomProperties and cpChildCustomProperties to be updated.
- Remember to also provide the cpChildId when updating a childTerm.

## when updating children of a child term
- The above conditions are also true. the syntax now has the prefix "ccp", i.e. ccpChildName. 

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

