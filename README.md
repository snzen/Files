# File utilities

[![Build status](https://ci.appveyor.com/api/projects/status/xn4k826bcvg3niaj?svg=true)](https://ci.appveyor.com/project/arsuq/files)

+ [.NET Framework 4.7.1 build](https://ci.appveyor.com/api/projects/arsuq/Files/artifacts/filesWin.zip)
+ [.NET Core 2.2 build](https://ci.appveyor.com/api/projects/arsuq/Files/artifacts/filesCore.zip)

Put the dlls in a PATH folder and launch from anywhere.
Some utils support changing the source dir, but the default is the current location.

Launch a specific subprogram with the -p switch:

.NET Framework 4.7.1

```
 files -p take
```

.NET Core 2.2

```
dotnet <path-to-filles>/files.dll -p move
```


All subprograms start in interactive mode, unless the -ni switch is present and supported.

```
files -p ext -ni -sp *.png -ext jpg
```

### duplicates
   Detects file duplicates in one or more folders by comparing sizes, names or data hashes.
   There are extension and size filters as well as option for partial hashing
   by skip/taking portions of the files.

### ext
   Recursively changes the extensions of all matched files.  
   Args:

	not interactive (-ni)
	source dir [default is current] (-src)
	search pattern [*.*] (-sp), extension (-ext)

### get 
   Downloads resources listed in a map file with each link on a separate line.  
   Args: 

	not interactive (-ni)
	links file (-f)
	base url (-base)
	destination dir (-dest)
	max req/sec [5.0] (-rps)
	from file row [0] (-from)
	to file row [last] (-to)

### insert
   Inserts/overrides the beginning or the end of all matching files with custom content.  
   Args:
	
	not interactive (-ni)
	source dir [default is current] (-src)
	search pattern [*.*] (-sp)
	ignore regex [optional Ex: (?:path\A|path\B)] (-ireg)
	recursive [y/*] (-r)
	text file [if not -txt] (-tf)
	text [if not -tf] (-txt)
	append [by default prepends] (-append)
	override [by default inserts] (-ovr)

### logrestore
   Creates a file with paths (log), which can be used to move files (restore).

### move
   Moves the matching files to DestinationDir/Prefix + Counter. Can be used as
   rename.
   Args:

    not interactive (-ni)
    source (-src)
    destination dir (-dest)
    prefix (-prf)
    indexing counter (-ic)
    ic step (-step)
    index length with zero padding (-zpad)
    sort options (-sort) 
         0 - no 
         1 - asc name
         2 - desc name 
         3 - randomize 
         4 - asc created date time (default)
         5 - desc created date time

### pad
   Adds leading zeros to the numbers in the file names.

### rename
   Renames the matching files from the current folder with the names from a text
   file.

### take
   Copies or moves n random files from the current folder into a destination folder.    
   Args:
	
	not interactive (-ni)
	source dir [default is current] (-src)
	destination dir (-dest)
	recursive [y/*] (-r)
	mirror dir tree [if recursive] (-mt)
	search pattern [*.*] (-sp)
	full-path regex [Ex: (?:dirA|dirB)] (-reg)
	ignore files smaller than [in KB] (-ist)
	ignore files bigger than [in KB] (-ibt)
	prefix the taken files with (-prf)
	take count (-take)
	move [default is copy mode] (-move)

### traverse 
   Traverses all sub-folders of a given root and launches a process with the provided args.
   If specified replaces a template string in the args with the current folder full path or name.   
   Args: 
 
    root (-root)
    program to call (-proc)
    program args (-pargs) Put in quotes and add a single space before the pargs string.
    current dir full path as arg name (-cdf)
    current dir as arg (-cd)
    Example:
	   Renames all files by prefixing them with their folder name.
       files -p traverse -ni -cdf $$ -cd $ -root <path> -proc <files>
       -pargs " -p move -ni -zpad 3 -src $$ -dest $$ -prf $-"
       Note the space before the -p move!
