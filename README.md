## Description
A pet ASP.NET Core 6 API project that allows house owners to find home renovation contractors by publishing advertisements and for contractors to find their clients by sending requests to ads.

## User types
<b>The system supports 4 main user types:
* Unauthorized user
* House owner
* Contractor
* Admin
</b>

Unauthorized user can:
* Have access to the list of ads
* Have access to a specific ad
* Login or Register

House owner inherits user permissions and can:
* Post their ads
* Edit their ads
* Cancel their ads
* See all pending requests for their ads
* Accept/Reject requests for their ads
* Rate contractors after they accomplished an ad <i>(not implemented yet)</i>
* Log out

Contractor inherits user permissions and can:
* Create requests for ads
* See all requests created by them
* Mark their accepted requests as accomplished
* Log out

Admin can additionally:
* Ban users <i>(not implemented yet)</i>

## Technical aspects

### Deployment
The application can be deployed as a Docker container and set up with docker-compose.yml file

### Data access layer
The application uses MSSQL and Entiy Framework Core with the Repository and Unit of Work pattern to talk to a database

### Authorization and authentication layer
The application uses ASP.NET Identity Framework to authorize and authenticate users

### Image storing
The application uses cloudinary service to remotely store images

### Unit testing
Unit testing is performed by using xUnit framework
