## Information about this project
This project is a container for shared class files between the launcher and the bootstrapper.
Because the application shares multiple AppDomains and different base directories we don't want add this project as a reference.
This would only result in loading the assembly to the CLR making it impossible to exchange at runtime via online updates. 
Instead classes in this project for the **Bootstrapper** shall be added **as a link**