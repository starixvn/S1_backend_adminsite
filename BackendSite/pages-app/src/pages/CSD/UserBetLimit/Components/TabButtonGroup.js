import React from 'react'

export default function TabButtonGroup ({ tpCategories, activeTab, clickTab }) {
    let checkedCategories = tpCategories.filter(a=>a.checked)
    return (
        <>
        {checkedCategories.map(item => {
            return (
                <button
                    className={`btn btn-add ${activeTab.id === item.categoryId ? 'active1' : ''}`}
				    key={`tpCategoryTab-${item.categoryId}`}
                    onClick={() => clickTab(item.categoryId,item.categoryName)} >
				    {item.categoryName}
			    </button>
            )
        })}
        </>
    );
}
