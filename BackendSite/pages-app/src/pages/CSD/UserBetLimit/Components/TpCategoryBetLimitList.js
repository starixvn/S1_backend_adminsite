import React from "react"
import SwitchButton from "./SwitchButton"
import InputBox from "./InputBox"

export default function TpCategoryBetLimitList({ categories, toggle, input }) {

    return (
        <>
            {categories.map((cate, i) => {
                let switchKey = `tpgame-switch-${cate.categoryId}`;
                return (
                    <div key={i} className="filter">
                        <div className="row-flex">
                            <div key={`tpCategory-${cate.categoryId}`} className="list__item" style={cate.checked ? { alignItems: "center", padding: "0.5rem", background: "#dcffee" } : { alignItems: "center", padding: "0.5rem" }}>
                                <label className="label">
                                    <span className="text">{cate.categoryName}</span>
                                </label>
                                <div className="switch__container">
                                    <input id={switchKey} className="switch switch--flat" type="checkbox" checked={cate.checked} onChange={(e) => toggle(e.target.id, cate.checked)}/>
                                    <label htmlFor={switchKey}></label>
                                </div>
                                <InputBox id={cate.categoryId} type="Number" title="MinBet" onChange={input} dataColumn="MinBet" disabled={!cate.checked} />
                                <InputBox id={cate.categoryId} type="Number" title="MaxBet" onChange={input} dataColumn="MaxBet" disabled={!cate.checked} />
                            </div>
                        </div>
                    </div>
                )}
            )}
        </>
    )
}