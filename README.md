# NpgsqlWrapper
This is a wrapper for Npgsql: [Npgsql](https://github.com/npgsql/npgsql)

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

The things you need before installing the software.

* PostgreSQL server and or PgAdmin
* The nuget Npgsql

### Installation

A step by step guide that will tell you how to get the development environment up and running.

```bash
# Clone this repository
$ git clone https://github.com/morre95/NpgsqlWrapper.git

# The first time you run this script edit and add this code to Main()
DatabaseConfig config = new DatabaseConfig
{
    Server = "localhost",
    Port = 5432,
    Username = "Username",
    Password = "Password",
    Database = "Database"
};

string configFile = "config.json";
DatabaseConfig.Save(configFile, config);

# Run that code only once. Then remove it so you don't have your password in plain text for security reason
```

The SQL table the test script is using, looks like this

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

## Usage

A few examples of useful code snipets.

## Example

```csharp
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

# Dump result set into list of Dictionary's
foreach (var item in await pgsql.DumpAsync("SELECT * FROM teachers WHERE id > @id", new DbParams("id", 1)))
{
    Console.WriteLine(item["first_name"]);
}
```


## Additional Documentation and Acknowledgments

* Wiki under construction
* etc...
