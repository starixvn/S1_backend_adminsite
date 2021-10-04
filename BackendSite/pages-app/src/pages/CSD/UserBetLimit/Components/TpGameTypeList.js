import React from 'react';
import InputBox from './InputBox';

export default function TpGameTypeList ({ categoryId,  gameTypeList, setBetLimit = f => f, toggle = g => g }) {
    return (
        <>
        {gameTypeList.map((game,i) => {
            let switchId = `${categoryId}-${game.typeId}`;
            let switchKey = `tpgame-switch-${categoryId}-${game.typeId}`;
            let minKey = `MinBet-${switchId}`
            let maxKey = `MaxBet-${switchId}`
			return (
				<div key={`tpgame-${game.typeId}`} className="list__item" style={game.checked ? { alignItems: "center", padding: "0.5rem", background: "#dcffee" } : { alignItems: "center", padding: "0.5rem" }}>
					<label className="label">
						<span className="text">{game.typeName}</span>
					</label>
					<div className="switch__container">
						<input id={switchKey} className="switch switch--flat" type="checkbox" checked={game.checked} onChange={() => toggle(switchId, game.checked)}/>
						<label htmlFor={switchKey}></label>
					</div>
					<InputBox title="Min Bet" value={game.minBet} type="Number" dataColumn={minKey} disabled={!game.checked} onChange={setBetLimit} />
                    <InputBox title="Max Bet" value={game.maxBet} type="Number" dataColumn={maxKey} disabled={!game.checked} onChange={setBetLimit} />
				</div>
			);
        })}
        </>
    )
}