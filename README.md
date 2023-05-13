<p align=center>
    <br>
    <span>Hunt down social media accounts by username across <a href="https://github.com/sherlock-project/sherlock/blob/master/sites.md">social networks</a></span>
    <br><br><br>
</p>

> Sherlock-cs is a reimagination of Sherlock that incorporates both concurrency and threading, enabling it to operate up to 400% faster. This program is compatible with the Sherlock format and automatically downloads the latest version from the Sherlock repository. Additionally, Sherlock-cs is licensed under AGPL-V3.

<br>

<p align="center">
    <img width="70%" height="70%" src="https://github.com/weiajr/Sherlock-cs/blob/master/images/speed.png"/>
</p>


## Usage
```console
./sherlock-cs USERNAMES [-h] [--folderoutput FOLDER]
              [--output OUTPUT] [-c CONCURRENCY]
              [-t TIMEOUT] [--print-all] [--print-found]
              [--local] [--nsfw] [--user-agent]


Usage: sherlock-cs [options] <username>

Arguments:

  username (Multiple)  <TEXT>

Options:

  -o | --output        <TEXT>
  If using single username, the output of the result will be saved to this file.

  -f | --folderoutput  <TEXT>
  If using multiple usernames, the output of the results will be saved to this folder.

  -c | --concurrency   <NUMBER>   [256]
  The degree of concurrency used more is faster

  --timeout            <DECIMAL>  [60]
  Time (in seconds) to wait for response to requests

  --print-all
  Output sites where the username was not found.

  --print-found                   [True]
  Output sites where the username was found.

  -l | --local
  Force the use of the local data.json file.

  --nsfw
  Include checking of NSFW sites from default list.

  --user-agent         <TEXT>     [Mozilla/5.0 (Macintosh; Intel Mac OS X 10.12; rv:55.0) Gecko/20100101 Firefox/55.0]
  Force the use of a custom user agent.
```

To search for only one user:
```
./sherlock-cs user1
```


To search for more than one user:
```
./sherlock-cs user1 user2 user3
```