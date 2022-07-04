import React from 'react';
import DropDownBox from './DropDownBox';

export default function TpGameTypeList({ categoryId, gameTypeList, groupChange = e => e, toggle = g => g }) {
	return (
		<>
			{gameTypeList.map((game, i) => {
				let switchId = `${categoryId}-${game.typeId}`;
				let switchKey = `tpgame-switch-${categoryId}-${game.typeId}`;
				let groupKey =  `dropDownBox-${categoryId}-${game.typeId}`;
				return (
					<div key={`tpgame-${game.typeId}`} className="list__item" style={game.checked ? { alignItems: "center", padding: "0.5rem", background: "#dcffee" } : { alignItems: "center", padding: "0.5rem" }}>
						<label className="label">
							<span className="text">{game.typeName}</span>
						</label>
						<div className="switch__container">
							<input id={switchKey} className="switch switch--flat" type="checkbox" checked={game.checked} onChange={() => toggle(switchId, game.checked)} />
							<label htmlFor={switchKey}></label>
						</div>
						{game.checked ? <DropDownBox
							columnTitle="LimitGroup"
							options={game.limitGroup}
							dataColumn="limitGroup"
							onChange={groupChange}
							groupKey={groupKey}
							value={game.selectedValue}
						/> : null}
					</div>
				);
			})}
		</>
	)
}