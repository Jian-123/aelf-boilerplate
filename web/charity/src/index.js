import React,{Component} from 'react';
import ReactDOM from 'react-dom';
import 'antd/dist/antd.css';
import './index.css';
import { Table, Form, Input, InputNumber, Button} from 'antd';

const columns = [
    { title: 'Time', dataIndex: 'time', key: 'time' },
    { title: 'Amount', dataIndex: 'amount', key: 'amount' },
    { title: 'From', dataIndex: 'from', key: 'from' },
    {
        title: 'To',
        dataIndex: '',
        key: 'x',
        render: () => <a>Delete</a>,
    },
];

const data = [
    {
        key: 1,
        time: 'John Brown',
        amount: 32,
        from: 'New York No. 1 Lake Park',
        description: 'My name is John Brown, I am 32 years old, living in New York No. 1 Lake Park.',
    },
    {
        key: 2,
        time: 'Jim Green',
        amount: 42,
        from: 'London No. 1 Lake Park',
        description: 'My name is Jim Green, I am 42 years old, living in London No. 1 Lake Park.',
    },
    {
        key: 3,
        time: 'Not Expandable',
        amount: 29,
        from: 'Jiangsu No. 1 Lake Park',
        description: 'This not expandable',
    },
    {
        key: 4,
        time: 'Joe Black',
        amount: 32,
        from: 'Sidney No. 1 Lake Park',
        description:"",
    },
];


function F1(input){
    let newData = {key:input.key, time:input.time, amount:input.amount, from:input.from, description:input.description,};
    data.push(newData);
}

let input = {
    key:10,
    time:"1111",
    amount:23,
    from:"fa",
    description:"af",
}

F1(input);


const layout = {
    labelCol: {
        span: 8,
    },
    wrapperCol: {
        span: 16,
    },
};
const validateMessages = {
    required: '${label} is required!',
    types: {
        email: '${label} is not validate email!',
        number: '${label} is not a validate number!',
    },
    number: {
        range: '${label} must be between ${min} and ${max}',
    },
};


class App extends Component{
    render(){
        const onFinish = values => {
            console.log(values);
        };


        return (
            <Form {...layout} name="nest-messages" onFinish={onFinish} validateMessages={validateMessages}>
        <Form.Item
    name={['user', 'name']}
    label="Name"
    rules={[
            {required: true,},
    ]}>
        <Input />
        </Form.Item>
        <Form.Item
        name={['user', 'email']}
        label="Email"
        rules={[
                {
                    type: 'email',
                },
    ]}
    >
    <Input />
        </Form.Item>
        <Form.Item
        name={['user', 'age']}
        label="Age"
        rules={[
                {
                    type: 'number',
                    min: 0,
                    max: 99,
                },
    ]}
    >
    <InputNumber />
        </Form.Item>

        <Form.Item name={['user', 'website']} label="Website">
        <Input />
        </Form.Item>
        <Form.Item name={['user', 'introduction']} label="Introduction">
        <Input.TextArea />
        </Form.Item>
        <Form.Item wrapperCol={{ ...layout.wrapperCol, offset: 8 }}>
            <Button type="primary" htmlType="submit"> Submit
            </Button>
        </Form.Item>
            </Form>




    );
    }
}


class Record extends Component{
    render(){
        return <Table
        columns={columns}
        expandable={{
            expandedRowRender: record => <p style={{ margin: 0 }}>{record.description}</p>,
            rowExpandable: record => record.decription !== "",
        }}
        dataSource={data} />

    }
}










ReactDOM.render(
<App />,
    document.getElementById('root')
);




/*
import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import * as serviceWorker from './serviceWorker';

ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root')
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
*/
