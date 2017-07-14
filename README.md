# RepoCrawler

RepoCrawler is a command line tool capable of crawling a list of repositories and build a new repository locally from all the packages found. It can be run multiple times and will only download the updated packages.

RepoCrawler will also generate the ``addons.xml`` file.

## Usage

1. [Install .NET Core](https://www.microsoft.com/net/core).
2. Download the code and restore the NuGet packages:

```
$ git clone https://github.com/Flavien/repo-crawler.git
$ cd repo-crawler/RepoCrawler
$ dotnet restore
```
3. Place the repository ZIP files inside the ```repositories``` directory:
```
$ mkdir repositories
```
4. Run RepoCrawler:
```
$ dotnet run
```

## License

Copyright 2017 Flavien Charlon

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and limitations under the License.
