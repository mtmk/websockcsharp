import Browser
import Html exposing (Html, button, div, text)
import Html.Events exposing (onClick)
import Html.Attributes exposing (class)

main =
  Browser.sandbox { init = 0, update = update, view = view }

type Msg = Increment | Decrement

update msg model =
  case msg of
    Increment ->
      model + 1

    Decrement ->
      model - 1



view model =
  div []
    [ button [ onClick Increment, class "f1 bg-blue" ] [ text "+" ]
    , div [ class "f1" ] [ text (String.fromInt model) ]
    , button [ onClick Decrement, class "f1 bg-red" ] [ text "-" ]
    ]
    