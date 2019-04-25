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

### Build  
To build docker image run from solution directory `docker build  -f ./SP_Taxonomy_client_test/Dockerfile .`

To build locally - use Visual Studio >=2019 Preview and .NET Core >= 3.0 Preview 

