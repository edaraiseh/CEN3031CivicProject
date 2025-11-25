# API Documentation
This file contains documentation for all the various API endpoints.

Base URL: https://PENDINGDOMAINNAME.com/api

---

## User Authentication
### GET `/users/{id}`
**Description:** Get a user by their ID.

**Responses:**
200 Ok
```json
{
    "id": 1,
    "username": "example_user",
    "name": "Example User",
    "profilepic": "https://optionalprofilepicturelink.com/",
    "bio": "This is my optional bio to be displayed on my account"
}
```
404 Not Found

### PUT `/users/register`
**Description:** Register a new user account.

**Request Body (JSON):**
```json
{
    "username": "example_user",
    "name": "Example User",
    "email": "example@email.com",
    "password": "P@ssw0rd!",
    "profilepic": "https://optionalprofilepicturelink.com/",
    "bio": "This is my optional bio to be displayed on my account"
}
```
Fields `username`, `name`, `email`, `password` are required, while `profilepic` and `bio` are optional and
will be set to null if left blank.

**Responses:**
200 Ok
```json
{
    "id": 1,
    "username": "example_user",
    "name": "Example User",
    "profilepic": "https://optionalprofilepicturelink.com/",
    "bio": "This is my optional bio to be displayed on my account"
}
```
400 Bad Request
409 Conflict

### POST `/users/login`
**Description:** Login using an existing user account and password.

**Request Body (JSON):**
```json
{
    "username": "example_user",
    "password": "P@ssw0rd!"
}
```

**Responses:**
200 Ok
```json
{
    "token": "jwttokenhere",
    "userid": 1 
}
```
400 Bad Request
401 Unauthorized

## User Posts
### GET `/posts`
**Description:** Get a selection of top level posts based on a query, optionally sorted.

**Parameters:**
|Name     |Type  |Required|Example    |Valid Values                                   |Default    |Description                                                                    |
|:--------|:-----|:-------|:----------|:----------------------------------------------|:----------|:------------------------------------------------------------------------------|
|page     |int   |true    |1          |page > 0                                       |1          |Group posts into groups of pageSize size and return the group with index `page`|
|pageSize |int   |true    |10         |pageSize > 0                                   |10         |Determine the size of group for grouping posts into pages                      |
|sortBy   |string|true    |"createdat"|"id","userid","content","createdat","updatedat"|"createdat"|Parameter to sort queried posts by                                             |
|sortOrder|string|true    |"desc"     |"asc","desc"                                   |"desc"     |Order to sort posts in                                                         |
|userId   |int   |false   |1          |userId >= 0                                    |null       |Optional parameter to limit posts to posts made by user with id `userid`       |
|search   |string|false   |"findthis" |any string                                     |null       |Optional parameter to limit posts to posts containing the string `search`      |

**Responses:**
200 Ok
```json
{
    "totalItems": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1,
    "posts": [
        {
            "id": 1,
            "author": "authory jones",
            "content": "Example post",
            "createdAt": "2025-10-29T05:21:58.121288Z",
            "updatedAt": null,
            "reactions": [] 
        }
    ]
}
```
400 Bad Request

### PUT `/posts`
**Description:** Create a post, requires a valid JWT.

**Request Body (JSON):**
```json
{
    "content": "This is an example post!"
}
```

**Responses:**
200 Ok
```json
{
    "id": 1,
    "content": "This is an example post!"
}
```
400 BadRequest
401 Unauthorized
409 Conflict

### DELETE `/posts/{postId}`
**Description:** Delete a post, requires a valid JWT and a valid target. If a post is a "top-level" post, ie it has no parent, then all child posts are recursively deleted in the entire hierarchy.


**Responses:**
200 Ok
```json
{
    "id": 1
}
```
401 Unauthorized
404 Not Found

### POST `/posts/{postId}`
**Description:** Update the contents of a post, requires a valid JWT and a valid target.

**Request Body (JSON):**
```json
{
    "newcontent": "This an updated post."
}
```

**Responses:**
200 Ok
```json
{
    "id": 1,
    "newcontent": "This an updated post."
}
```
401 Unauthorized
404 Not Found

### PUT `/posts/{postId}/replies`
**Description:** Create a reply to an existing post or reply, requires a valid JWT and a valid target.

**Request Body (JSON):**
```json
{
    "content": "This a reply to another post."
}
```

**Responses:**
200 Ok
```json
{
    "id": 2,
    "content": "This a reply to another post."
}
```
401 Unauthorized
404 Not Found

### GET `/posts/{postId}/replies`
**Description:** Get all replies to a given post, requires a valid target. Deleted posts are also returned, but their contents are set to `null`.

**Responses:**
200 Ok
```json
{
    [
        {
            "id": 2,
            "content": "This a reply to another post."
        },
        {
            "id": 3,
            "content": "This is also a reply to another post."
        }
    ]
}
```
400 Bad Request
404 Not Found

### GET `/posts/{postId}/reactions`
**Description:** Get all reaction aggregates to a given post, requires a valid target. 

**Responses:**
200 Ok
```json
{
    "postid": 1,
    "likes": 3,
    "dislikes": 1,
    "hearts": 2 
}
```
404 Not Found

### PUT `/posts/{postId}/reactions`
**Description:** Create a reaction to a specified post, requires a valid target and a valid JWT. 

**Request Body (JSON):**
```json
{
    "type": "like"
}
```
Note that type is a stringly-typed enum, valid values are: `like`, `dislike`, `heart`.

**Responses:**
200 Ok
```json
{
    "postid": 1,
    "type": "like"
}
```
404 Not Found
409 Conflict

### DELETE `/posts/{postId}/reactions`
**Description:** Delete a reaction to a specified post, requires a valid target, a valid JWT, and that the specified reaction exists. 

**Request Body (JSON):**
```json
{
    "type": "like"
}
```
Note that type is a stringly-typed enum, valid values are: `like`, `dislike`, `heart`.

**Responses:**
200 Ok
```json
{
    "postid": 1,
    "type": "like"
}
```
404 Not Found
## Official Posts
Official posts are just a special type of post, most of their operations use the default post routes. The only different routes are shown below:

### GET `/posts/official`
**Description:** Get a selection of official posts based on a query, optionally sorted.

**Parameters:**
|Name     |Type  |Required|Example    |Valid Values                                   |Default    |Description                                                                    |
|:--------|:-----|:-------|:----------|:----------------------------------------------|:----------|:------------------------------------------------------------------------------|
|page     |int   |true    |1          |page > 0                                       |1          |Group posts into groups of pageSize size and return the group with index `page`|
|pageSize |int   |true    |10         |pageSize > 0                                   |10         |Determine the size of group for grouping posts into pages                      |
|sortBy   |string|true    |"createdat"|"id","userid","content","createdat","updatedat"|"createdat"|Parameter to sort queried posts by                                             |
|sortOrder|string|true    |"desc"     |"asc","desc"                                   |"desc"     |Order to sort posts in                                                         |
|userId   |int   |false   |1          |userId >= 0                                    |null       |Optional parameter to limit posts to posts made by user with id `userid`       |
|search   |string|false   |"findthis" |any string                                     |null       |Optional parameter to limit posts to posts containing the string `search`      |

**Responses:**
200 Ok
```json
{
    "totalItems": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1,
    "posts": [
        {
            "id": 1,
            "content": "Example post",
            "createdAt": "2025-10-29T05:21:58.121288Z",
            "updatedAt": null,
            "reactions": [] 
        }
    ]
}
```
400 Bad Request

### PUT `/posts/official`
**Description:** Create an official post, requires a valid JWT and that the user account backed by the JWT is an official user.

**Request Body (JSON):**
```json
{
    "content": "This is an example post!"
}
```

**Responses:**
200 Ok
```json
{
    "id": 1,
    "content": "This is an example post!"
}
```
400 BadRequest
401 Unauthorized
409 Conflict


## Petitions
### GET `/petitions`
**Description:** Get a selection of petitions based on a query, optionally sorted.

**Parameters:**
|Name     |Type  |Required|Example    |Valid Values                                   |Default    |Description                                                                    |
|:--------|:-----|:-------|:----------|:----------------------------------------------|:----------|:------------------------------------------------------------------------------|
|page     |int   |true    |1          |page > 0                                       |1          |Group posts into groups of pageSize size and return the group with index `page`|
|pageSize |int   |true    |10         |pageSize > 0                                   |10         |Determine the size of group for grouping posts into pages                      |
|sortBy   |string|true    |"createdat"|"id","userid","content","createdat","updatedat"|"createdat"|Parameter to sort queried posts by                                             |
|sortOrder|string|true    |"desc"     |"asc","desc"                                   |"desc"     |Order to sort posts in                                                         |
|userId   |int   |false   |1          |userId >= 0                                    |null       |Optional parameter to limit posts to posts made by user with id `userid`       |
|search   |string|false   |"findthis" |any string                                     |null       |Optional parameter to limit posts to posts containing the string `search`      |

**Responses:**
200 Ok
```json
{
    "totalItems": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1,
    "petitions": [
        {
            "id": 1,
            "title": "Example Petition",
            "content": "This is an example petition",
            "createdAt": "2025-10-29T05:21:58.121288Z",
            "updatedAt": null,
            "signaturecount": 0,
            "author": "authory jones",
        }
    ]
}
```
400 Bad Request

### PUT `/petitions`
**Description:** Create a petition, requires a valid JWT.

**Request Body (JSON):**
```json
{
    "title": "Example Petition",
    "content": "This is an example petition!"
}
```

**Responses:**
200 Ok
```json
{
    "id": 1,
    "title": "Example Petition",
    "content": "This is an example petition!"
}
```
400 BadRequest
401 Unauthorized
409 Conflict

### DELETE `/petitions/{petitionId}`
**Description:** Marks a petition as failed, requires a valid JWT and a valid target. 


**Responses:**
200 Ok
```json
{
    "id": 1
}
```
401 Unauthorized
404 Not Found

### PUT `/petitions/{petitionId}/replies`
**Description:** Create a reply to an existing petition(NOT a petition reply), requires a valid JWT and a valid target.

**Request Body (JSON):**
```json
{
    "content": "This a reply to a petition."
}
```

**Responses:**
200 Ok
```json
{
    "id": 2,
    "content": "This a reply to another post."
}
```
401 Unauthorized
404 Not Found

### GET `/petitions/{petitionId}/replies`
**Description:** Get all replies to a given petition, requires a valid target. Deleted replies are also returned, but their contents are set to `null`.

**Responses:**
200 Ok
```json
{
    [
        {
            "id": 2,
            "content": "This a reply to a petition."
        },
        {
            "id": 3,
            "content": "This is also a reply to the same petition."
        }
    ]
}
```
400 Bad Request
404 Not Found

### PUT `/petitions/{petitionId}/sign`
**Description:** Signs a petition. Requires a valid target, a JWT, and that the user account denoted by the JWT has not already signed the petition. 

**Responses:**
200 Ok
```json
{
    "petitionid": 1,
    "userid": 2
}
```
404 Not Found
409 Conflict

### DELETE `/petitions/{petitionId}/sign`
**Description:** Deletes a petition signature. Requires a valid target, a JWT, and that the user account denoted by the JWT has signed the petition. 

**Responses:**
200 Ok
```json
{
    "petitionid": 1,
    "userid": 2
}
```
404 Not Found