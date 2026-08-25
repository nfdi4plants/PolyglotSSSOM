namespace SSSOM

type MappingCardinalityEnum =
    | OneToOne
    | OneToMany
    | ManyToOne
    | ManyToMany
    | OneToNone
    | NoneToOne
    | NoneToNone

module MappingCardinalityEnum =
    let create (text: string) =
        match text with
        | "1:1" -> OneToOne
        | "1:n" -> OneToMany
        | "n:1" -> ManyToOne
        | "n:n" -> ManyToMany
        | "1:0" -> OneToNone
        | "0:1" -> NoneToOne
        | "0:0" -> NoneToNone
        | unknown -> failwith $"Can't parse MappingCardinalityEnum. Unknown value: '{unknown}'"


    let toString (enumValue: MappingCardinalityEnum) =
        match enumValue with
        | OneToOne ->   "1:1"
        | OneToMany ->  "1:n"
        | ManyToOne ->  "n:1"
        | ManyToMany -> "n:n"
        | OneToNone ->  "1:0"
        | NoneToOne ->  "0:1"
        | NoneToNone -> "0:0"

