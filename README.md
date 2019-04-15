![alt text](./images/dmn-logo.png "Logo Title Text 1")

# DMN Testing Utility

A simple, yet effective command line utility for creating test scenario's for DMN models.

## Status

Under construction, do not use for production purposes.

## Dependencies

* Dot Net Core >= 2.2
* CommandLineParser >=2.4.3
* net.adamec.lib.common.dmn.engine >= 0.1.0
* Newtonsoft.Json" >= 12.0.1
* Camunda
* Docker

## Workings

To get you started, the following will describe a way of working with the command line utility in your project.

### Usage, best practices

#### DMN Modelling

An analist/developer typically models a decision model using DMN tools, such as available from:

* The Camunda Modeler (Desktop)
* The Camunda Dmn Simulator (Web)

#### Storing DMN for execution

DMN's are then stored, together with BPMN processes if any, in the Camunda Engine. For more information on running camunda from a docker image please visit this project: https://github.com/camunda/docker-camunda-bpm-platform

or quickly install it if you know what you are doing:

```bash
docker run -d --restart unless-stopped --name camunda -p 8080:8080 camunda/camunda-bpm-platform:latest
```



#### Generate test templates

The commandline utility is able to discover all the DMN models that are stored in the Camunda engine. It does this by querying Camunda's RESTful API endpoint. The endpoint needs to be added as a parameter to the Command Line utility using the -e flag.

i.e:

```bash
foo@bar:docker run --rm=true --name dmn-test -v /home/$USER:/tmp wigo4it/dmn-test -o create -k invoiceClassification -e http://192.168.178.50:8080/engine-rest -m /tmp/testtemplate.md
```

Here's the output of that command in [testtemplate.md](src/dmn-test/testtemplate.md)

This utility will discover all the input / output paramters of your models and create empty test stubs for you in the form of markdown tables, which you can then fill in with test data. These markdown files are easily used in your own source code projects.

#### Test Tables

Here's an example of a test table that is discoverable by this utility from your project's markdown files.

```markdown
## dmn:invoiceClassification
| Invoice Amount | Invoice Category | *Classification                  |!|
|---------------:|:-----------------|:--------------------------------:|:|
| 200            | "Misc"           | "day-to-day expense"             |O|
| 250            | "Misc"           | "budget"                         |X|
| 10000          | "Misc"           | "exceptional"                    |O|
```
In this test secnario, the test data is provided in the 'Invoice Amount' and 'Invoice Category' columns. The columns are input parameters for the DMN table in Camunda. The expected output values are denoted in the 'Classification' column. Notice the asterisk * in front of the column name. This means the utlity treats this as an output parameter.

The last column '!' contains the test results.

| Invoice Amount | Invoice Category | *Classification                  | ! |
|---------------:|:-----------------|:--------------------------------:|:-:|
| 200            | "Misc"           | "day-to-day expense"             |&#x1F49A;|
| 250            | "Misc"           | "budget"                         |&#x1F534;|
| 10000          | "Misc"           | "exceptional"                    |&#x1F49A;|
##### actual visualization in markdown



### DMN Interpretation and execution

C# Dmn engine is used for interpreting metadata from the DMN model. Camunda is actually used for execution. While the C# Dmn engine can execute only FEEL expressions, Camunda also has support for more advanced expressions for interpreters such as Groovy Script.

