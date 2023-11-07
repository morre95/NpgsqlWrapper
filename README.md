# NpgsqlWrapper
A short description about the project and/or client.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

The things you need before installing the software.

* The nuget Npgsql

### Installation

A step by step guide that will tell you how to get the development environment up and running.

```bash
# Clone this repository
$ git clone https://github.com/morre95/NpgsqlWrapper.git

# Create a .env file in projectFolder/bin/net7.0
$ type nul > .env

# Edit .env file
dbHost=ip to host:port
bdUsername=username to your server
dbPassword=password to your server
dbDatabase=database
```

## Usage

A few examples of useful code snipets.

```sql
# Sql create statment for running the example
CREATE TABLE teachers
(
    id serial NOT NULL,
    first_name character varying(25),
    last_name character varying(25),
    subject character varying(20),
    salary integer,
    PRIMARY KEY (id)
);
```

## Example

```c#
# Init
MyNpgsqlAsync pgsql = new(host, username, password, database);

# Insert
Actor act = new Actor();
act.first_name = "First name";
act.last_name = "Last name";
act.last_update = DateTime.Now;
await pgsql.InsertAsync(act);

# Delete
DbParams p = new("id", id);
await pgsql.DeleteAsync<Teachers>($"id = @id", p);

# Update command
var teacherToEdit = new Teachers()
{
    first_name = firstName,
    last_name = lastName,
    subject = subject,
    salary = salary
};
DbParams p = new("id", id);
await pgsql.UpdateAsync(teacher, "id=@id", p);

# Fatch many
List<Actor> actors = await pgsql.FetchAsync<Actor>();
foreach (Actor actor in actors)
{
    Console.WriteLine(actor.first_name);
}

# Fetch one result
Film film = await pgsql.ExecuteOneAsync<Film>(); // Eqvivalent to SELECT * FROM film LIMIT 1
Console.WriteLine($"id = {film.film_id}, title = {film.title},  +
$"length = {TimeSpan.FromMinutes(film.length).ToString(@"hh\:mm")}");
```

## Deployment

Additional notes on how to deploy this on a live or release system. Explaining the most important branches, what pipelines they trigger and how to update the database (if anything special).

### Branches

* Master:
* Feature:
* Bugfix:
* etc...

## Additional Documentation and Acknowledgments

* Project folder on server:
* Confluence link:
* Asana board:
* etc...
