import React from 'react';

const breadCrumb = (props)=>{
    let contentRow = props.BreadCrumbs.map((item, idx)=>{
        return(
            <div className="newbreadcrumb__item" key={`breadCrumbs-${props.ItemKey}-${idx}`}>
                <div className="newbtn-area">
                    <span className="text">{item.DisplayText}</span>
                </div>
            </div>
        )
    });
    return(
        <div className="newbreadcrumb">
            {contentRow}
        </div>
    )
}

export default breadCrumb;