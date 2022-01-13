import React from 'react'

export default function TabButtonGroup ({ currencyOptions, activeTab, clickTab }) {
    return (
        <>
        {currencyOptions.map(item => {
            return (
                <button
                    className={`btn btn-add ${activeTab.currencyId === item.value ? 'active1' : ''}`}
				    key={`tpCategoryTab-${item.value}`}
                    onClick={() => clickTab(item.value, item.displayText)} >
				    {item.displayText}
			    </button>
            )
        })}
        </>
    );
}
