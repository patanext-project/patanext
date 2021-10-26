dotnet paket install
dotnet paket update
dotnet pack paket-files/github.com/guerro323/GameHost/ -o ./packages --version-suffix 42
dotnet pack paket-files/github.com/guerro323/revtask/ -o ./packages --version-suffix 42
dotnet pack paket-files/github.com/guerro323/revecs/ -o ./packages --version-suffix 42
dotnet restore