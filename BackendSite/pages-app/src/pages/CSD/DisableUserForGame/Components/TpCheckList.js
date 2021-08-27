import React, {useState}  from "react"
import Checkbox from "./Checkbox"

export default function TpCheckList({ tpList, tpChecked = f => f, gameChecked = g => g }) {

    return (
        <>
            {tpList.map((tp, i) => { 
                return (
                    <div key={i} className="filter">
                        <Checkbox
                            key={i}
                            title={tp.tpName}
                            value={tp.tpId}
                            checked={tp.checked}
                            toggle = {tpChecked}
                            />
                            <div>
                        {(tp.gameList != null && tp.gameList.length > 0) ? tp.gameList.map((game, j) => (
                            <Checkbox
                            key={j}
                            title={game.name}
                            value={tp.tpId + "-" + game.gameId}
                            checked={game.checked}
                            toggle = {gameChecked}
                            />
                        )) : null} 
                        </div>
                    </div>
                )
            }
        )}
    </>
            )
}