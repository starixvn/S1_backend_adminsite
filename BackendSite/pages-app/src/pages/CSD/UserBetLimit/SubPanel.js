import React, { useState, useEffect, useReducer } from "react";
import TabButtonGroup from "./Components/TabButtonGroup"
import InputBox from "./Components/InputBox";
import TpGameTypeList from "./Components/TpGameTypeList"

function SubPanel ( {tpGames, currentTab, clickTab, setBetLimit, enableGameType, setTpBetLimit}) {
    let idx = tpGames.findIndex(x => x.categoryId === currentTab.id);
    let category = tpGames[idx];
    let minKey = `MinBet-${currentTab.id}`;
    let maxkey = `MaxBet-${currentTab.id}`;
    return (
        <>
        <div className="filter noBGcolor tag">
            <TabButtonGroup tpCategories={tpGames} activeTab={currentTab} clickTab={clickTab}/>
        </div>
        <div className="filter">
            <div className="panel level-1 mt-20">
                <a className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
				    <span style={{ color: "white" }}>{currentTab.name}</span>
			    </a>
                <div className="panel-body">
                    <div className="row-flex">
                        <InputBox type="Number" value={category.minBet} title="Min Bet" dataColumn={minKey} onChange={setBetLimit} />
                        <InputBox type="Number" value={category.maxBet} title="Max Bet" dataColumn={maxkey} onChange={setBetLimit} />
                    </div>
                    <div className="row-flex">
                        <div className="input-item">
                            <label className="label">Tp Games</label>
                        </div>
                    </div>
                    <div className="row-flex">
                        <TpGameTypeList categoryId={currentTab.id} gameTypeList={category.gameTypeBetLimit} setBetLimit={setTpBetLimit} toggle={enableGameType} />
                    </div>
                </div>
            </div>  
        </div>
        </>
    )
}

export default SubPanel