export const dateFormat = (createTime)=>{
    return moment(createTime).format('l LTS');
}

export const boolFormat = (v)=>{
    return v ? 'true' : 'false';
}

export const columnDefs = [
    {
        dataColumn:'createTime', 
        columnTitle:`CreateTime ${moment().format('(Z)')}`,
        body:{
            format:dateFormat
        }
    },
    {
        dataColumn:'flag', 
        columnTitle:'Read',
        body:{
            format:boolFormat
        }
    },
    { dataColumn:'title',  columnTitle:'Subject' },
    { dataColumn:'content',  columnTitle:'Content' },
    { dataColumn:'sender',  columnTitle:'Sender' }
];