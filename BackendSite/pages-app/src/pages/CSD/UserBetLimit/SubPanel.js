import React, { useState, useEffect, useReducer } from "react";
import TabButtonGroup from "./Components/TabButtonGroup"
import TpGameTypeList from "./Components/TpGameTypeList"

function SubPanel({ tpGames, currentTab, clickTab, groupChange, enableGameType }) {
    let idx = tpGames.findIndex(x => x.categoryId === currentTab.id);
    let category = tpGames[idx];
    return (
        <>
            <div className="filter noBGcolor tag">
                <TabButtonGroup tpCategories={tpGames} activeTab={currentTab} clickTab={clickTab} />
            </div>
            <div className="filter">
                <div className="panel level-1 mt-20">
                    <a className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
                        <span style={{ color: "white" }}>{currentTab.name}</span>
                    </a>
                    <div className="panel-body">
                        <div className="row-flex">
                            <div className="input-item">
                                <label className="label">Tp Games</label>
                            </div>
                        </div>
                    </div>
                    <div className="row-flex">
                        <TpGameTypeList categoryId={currentTab.id} gameTypeList={category.gameTypeBetLimit} groupChange={groupChange} toggle={enableGameType} />
                    </div>
                </div>
            </div>
        </>
    )
}

export default SubPanel