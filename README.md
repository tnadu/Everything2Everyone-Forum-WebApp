# E2E (EverythingToEveryone) - Forum WebApp

## General Description
Everything2Everyone is a Crowd Knowledge Contribution and Distribution platform, designed 
for like-minded individuals of all kinds. The app is structured around articles which belong 
to various categories you can pick and choose from, just like a forum. Multiple chapters are 
associated with every article and a versioning system was specifically designed with article
management in mind. Every user can share their thoughts and further enrich the content of an
article by posting comments in the dedicated section of each article.

#### A sample article page containing its associated comments and various management functions (edit, delete, restrict)
![alt text](https://images2.imgbox.com/f9/b5/qyQGBUlx_o.png)


## Notable Features
* articles can be filtered by category and sorted chronologically and alphabetically
![alt text](https://images2.imgbox.com/b7/6f/NZfCl66q_o.png)

* an article search function which matches the provided string sequence with article titles and chapter contents

* whenever creating a new article or editing an existing one, chapters can be managed dynamically and the user can style the content of each chapter using a built-in markup editor
![alt text](https://images2.imgbox.com/b7/2d/Qo8EiTrR_o.gif)

* articles support a linear versioning system similar to a git branch (every version is a commit with a distinct title; past versions cannot be altered, but can be checked out for future versions)
![alt text](https://images2.imgbox.com/31/12/bmr7uB3q_o.gif)

* 3 types of users with various permissions and access levels: regular, editor, administrator

* profile settings and a dedicated user management panel for administrators
![alt text](https://images2.imgbox.com/74/3d/e7lDPIKK_o.gif)

* if an administrator's account was compromised and the bad actor intends to 
do reconnaissance by trying random IDs in the user edit URL, they won't be 
prompted an error message which would clearly indicate that the user doesn't
exist, making it more difficult for them to script such a technique (fictitious 
users are created on demand)


## Dependencies
All of the following packages were installed using the NuGet packet manager:
* Entity Framework Core - Diagnostics, SqlServer, Tools
* Identity Framework


## Building and Running
The project was built and run exclusively using Visual Studio. The following SDKs should be installed:
* ASP.NET and web development
* .NET desktop development
* Universal Windows Platform development
* Data storage and processing


## Project Contributors
* playback0022
* VladWero08
