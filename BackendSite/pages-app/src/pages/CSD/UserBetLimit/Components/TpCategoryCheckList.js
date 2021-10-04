import React from 'react';
import Checkbox from './Checkbox'

export default function TpCategoryCheckList ({ title, tpCategoryList, toggle = f => f }) {
    let tpCategories = tpCategoryList.map((tp, i)=>{
        return (
            <Checkbox 
                key={i}
                title={tp.categoryName}
                value={tp.categoryId}
                checked={tp.checked}
                toggle = {toggle} />)
    });
    return(
        <>
            <div className="row-flex">
                <div className="input-item">
                    <label className="label">{title}</label>
                </div>
            </div>
            <div className="row-flex">{tpCategories}</div>
        </>
    )
}