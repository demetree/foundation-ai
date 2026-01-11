# Foundation Platform Design Notes

The Foundation provides a platform upon which to develop C# web applications that use a database to store their data.

It provides features and tools to be able to rapidly and reliably create a system that is complete enough to 
use as-is, or to extend by adding custom logic and screens as necessary.

It can do this because it provides all of the cornerstone features that complex systems require.

The features that it provides to any application that is built on it are:

- User Management
- Role and permission Management
- Access Auditing
- Automated creation of all database creation and security setup scripts
- Automated creation of all CRUD operations
- Optional multiple tenancy support
- Optional data visibility system for grouping users and data
- Optional data version control
- Optional messaging system
- Optional notification system


# Developing an application on the Foundation platform

## Design Premise

1. All applications will use the Foundation Security module, and therefore don't need additional security related considerations in their databases.
2. All applications will use the Foundation Auditor module, and therefore don't need additional access tracking related considerations in their databases.
3. An application built on the Foundation starts with the design of its own application specific database.
4. Foundation Code Generation tools, and Foundation Libary referneces provide the basic services.
4. Custom development then extends to add or change features as necessary.


## Foundation Security Module


### Security Privileges

The Foundation prescribes 6 standard security privileges.  

These are:

| ID | Name          | Description 	                                         | Comments                   |
|----|---------------|-------------------------------------------------------|----------------------------|
|1|No Access|No Access|Foundation explicitly denies access if this privilege is on a user, overriding any other.|
|2|Anonymous Read Only|Read Only Access, With All Sensitive Data Redacted|Each application must custom define what it considers confidential, and then obscure that.  Foundation services are not in place for automation of this.|
|3|Read Only|Read Only Access For General Use|Foundation will only allow reads.|
|4|Read and Write|Read and Write Access|Foundation will allow reads and writes.|
|5|Administrative|Complete Administrative Access|Foundation will not limit access in any way.|
|6|Custom|Custom Access Level|This privilege is to be used for custom security roles that provide application specific functionality.|

Of these 6 roles, 4 of them are fully integrated into the Foundation services.  The fully integrated privileges are:

1. No Access
3. Read Only
4. Read and Write
5. Administrative

The remaining priviles are to be used as follows:

2. Anonymous Read Only 
    - Generally not to be used.  Pending a use case that requires it, and it can be integrated into the Foundation if and when necessary.
6. Custom 
   - To be used when defining any custom role for an application specific purpose.



Security privileges are attached to a security role, where one role has one privilege assigned.

### Default Application Security Roles

The Foundation's initial configuration for application is to provide several standard security roles.  

The standard security roles created by the Foundation are to suit basic needs to serve initial workflows.  The standard roles follow the standard security privileges as follows:


Security roles are attached to a user, and a user can have multiple roles.



### Security Level for table access

The Foundation provides table level access control using a permission level number between 0 and 255.  Each table in a Foundation application can be given a minimum read and write value.
- A user attempting to read from the table must have at least the minimum read security level that the table requires to see data from the table.  
- A user attempting to write to the table must have at least the minimum write security level that the table requires to write to the table.  

Each user record has a 

###

## Foundation Auditor Module


## Application Specific Database



# Foundation Development Tools


## Database Modeling Tools

### Database Script Generation

### Security Script Generation

### Default Data Script Generation


## Version Control

## Multi-Tenancy

## Data Visibility 


## Application Code Generation Tools

### .Net Framework 4.8 and AngularJS


### .Net 8 and Angular


# Next Steps In Application Development

## Design Custom Workflow and Business Logic Needs

All applications must have their own requirements well defined.  With the requirements defined, decisions can be made as to whether or not a Foundation provided
service is sufficient, or whether extended screens and business logic is necessary.



