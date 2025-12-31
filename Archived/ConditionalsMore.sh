read X
read Y
read Z

if (( X != Y && Y != Z && Z != X )); then
    echo "SCALENE"
fi

if (( X == Y && Y == Z && Z == X )); then
    echo "EQUILATERAL"
fi

if (( (X == Y && X != Z) || (Y == Z && Z != X) || (Z == X && X != Y) )); then
    echo "ISOSCELES"
fi
