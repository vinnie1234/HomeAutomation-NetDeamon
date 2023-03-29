(dotnet restore)

regex='PackageReference Include="([^"]*)" Version="([^"]*)"'

echo 'Updating nuget packages...'
for csproj in ./*.csproj; do
    csprojFullPath=$(readlink -f "$csproj")
    while read -r line; do
        if [[ $line =~ $regex ]]; then
            echo "line" + $line
            package="${BASH_REMATCH[1]}"
            echo "package" + $package
            echo "BASH_REMATCH0" + ${BASH_REMATCH[0]}
            echo "BASH_REMATCH1" + ${BASH_REMATCH[1]}
            echo "BASH_REMATCH2" + ${BASH_REMATCH[2]}
            echo "BASH_REMATCH3" + ${BASH_REMATCH[3]}
            echo "BASH_REMATCH4" + ${BASH_REMATCH[4]}
            echo ${BASH_REMATCH[1]}
            dotnet add "$csprojFullPath" package "$package"
        fi
    done <"$csproj"
done